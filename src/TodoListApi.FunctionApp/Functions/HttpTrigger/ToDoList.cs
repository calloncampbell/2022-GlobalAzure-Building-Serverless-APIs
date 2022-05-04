using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using TodoListApi.FunctionApp.Abstractions.Interfaces;
using TodoListApi.FunctionApp.Abstractions.Models;

namespace TodoListApi.FunctionApp.Functions.HttpTrigger
{
    public class ToDoList
    {
        private readonly ILogger<ToDoList> _logger;
        private readonly IToDoListService _todoService;

        public ToDoList(
            ILogger<ToDoList> log,
            IToDoListService toDoListService)
        {
            _logger = log;
            _todoService = toDoListService;
        }

        [FunctionName("GetTodoItems")]
        [OpenApiOperation(operationId: "GetTodoItems")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ToDoItem[]), Description = "A to do list")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")] 
            HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            IActionResult returnValue = null;

            try
            {
                var results = await _todoService.GetToDoItemsAsync();
                if (results == null)
                {
                    _logger.LogInformation($"There are no items in the collection");
                    returnValue = new NotFoundResult();
                }
                else
                {
                    returnValue = new OkObjectResult(results);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }

        [FunctionName("GetTodoItem")]
        [OpenApiOperation(operationId: "GetTodoItem")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "To do item id")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ToDoItem), Description = "A to do item")]        
        public async Task<IActionResult> GetTodoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/{id}")]
            HttpRequestMessage req, 
            string id)
        {

            IActionResult returnValue = null;

            try
            {
                var result = await _todoService.GetToDoItemAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("That item doesn't exist!");
                    returnValue = new NotFoundResult();
                }
                else
                {
                    returnValue = new OkObjectResult(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Couldn't find item with id: {id}. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }

        [FunctionName("PostTodoItem")]
        [OpenApiOperation(operationId: "PostTodoItem")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ToDoItem), Required = true, Description = "To do object that needs to be added to the list")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ToDoItem), Description = "A to do item")]        
        public async Task<IActionResult> PostTodoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todos")] 
            HttpRequest req)
        {
            IActionResult returnValue = null;

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var input = JsonConvert.DeserializeObject<ToDoItem>(requestBody);                
                var result = await _todoService.UpsertToDoItemAsync(input);

                _logger.LogInformation("Todo item inserted");
                returnValue = new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not insert item. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }

        [FunctionName("PutTodoItem")]
        [OpenApiOperation(operationId: "PutTodoItem")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "To do Id")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ToDoItem), Required = true, Description = "To do object that needs to be updated to the list")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ToDoItem), Description = "A to do item")]        
        public async Task<IActionResult> PutTodoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todos/{id}")] 
            HttpRequest req,
            string id)
        {
            IActionResult returnValue = null;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedResult = JsonConvert.DeserializeObject<ToDoItem>(requestBody);
            updatedResult.Id = id;

            try
            {
                var replacedItem = await _todoService.UpsertToDoItemAsync(updatedResult);
                if (replacedItem == null)
                {
                    returnValue = new NotFoundResult();
                }
                else
                {
                    returnValue = new OkObjectResult(updatedResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not update Album with id: {id}. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }

        [FunctionName("DeleteTodoItem")]
        [OpenApiOperation(operationId: "DeleteTodoItem")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "To do Id that needs to be removed from the list")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "OK Response")]        
        public async Task<IActionResult> DeleteTodoItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todos/{id}")]
            HttpRequestMessage req, 
            string id)
        {
            IActionResult returnValue = null;

            try
            {
                var itemToDelete = await _todoService.DeleteToDoItemAsync(id);                

                if (itemToDelete == null)
                {
                    _logger.LogInformation($"Todo item with id: {id} does not exist. Delete failed");
                    returnValue = new StatusCodeResult(StatusCodes.Status404NotFound);
                }

                returnValue = new StatusCodeResult(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not delete item. Exception thrown: {ex.Message}");
                returnValue = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return returnValue;
        }
    }
}

