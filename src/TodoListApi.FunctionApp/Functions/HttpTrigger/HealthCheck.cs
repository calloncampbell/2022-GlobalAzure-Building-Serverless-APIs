using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace TodoListApi.FunctionApp.Functions.HttpTrigger
{
    public class HealthCheck
    {
        private readonly ILogger<HealthCheck> _logger;

        public HealthCheck(ILogger<HealthCheck> log)
        {
            _logger = log;
        }

        [OpenApiOperation(operationId: nameof(HealthCheck))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "Status code 200")]
        [FunctionName(nameof(HealthCheck))]
        public async Task<IActionResult> HealthCheckAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "head",
            Route = "todos")]HttpRequestMessage req)
        {
            IActionResult returnValue = null;

            returnValue = new StatusCodeResult(StatusCodes.Status200OK);

            return returnValue;
        }
    }
}

