using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TodoListApi.FunctionApp.Abstractions.Constants;
using TodoListApi.FunctionApp.Abstractions.Interfaces;
using TodoListApi.FunctionApp.Abstractions.Models;

namespace TodoListApi.FunctionApp.Services
{
    public class ToDoListService : IToDoListService
    {
        private CosmosClient _cosmosClient;
        private readonly ILogger<ToDoListService> _logger;
        private readonly IConfigurationRoot _configuration;

        public ToDoListService(
            CosmosClient cosmosClient,
            ILogger<ToDoListService> log,
            IConfigurationRoot configuration)
        {
            _cosmosClient = cosmosClient;
            _configuration = configuration;
            _logger = log;
        }

        public async Task<List<ToDoItem>> GetToDoItemsAsync()
        {
            try
            {
                var container = _cosmosClient.GetContainer(Constants.CosmosDb.DatabaseId, Constants.CosmosDb.Collection.ToDoItemCollection);
                var query = $"SELECT * FROM c WHERE c.type = 'todoItem')";
                var options = new QueryRequestOptions { PartitionKey = new PartitionKey("id") };
                var iterator = container.GetItemQueryIterator<ToDoItem>(query, requestOptions: options);

                var locations = new List<ToDoItem>();
                var cost = 0d;
                while (iterator.HasMoreResults)
                {
                    var page = await iterator.ReadNextAsync();
                    cost += page.RequestCharge;
                    locations.AddRange(page);
                }

                _logger.LogWarning("Range read executed for item type ToDoItem total RU cost {cost}", cost);

                return locations.OrderBy(x => x.CreatedDateUtc).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetToDoItemsAsync error");
                throw;
            }
        }

        public async Task<ToDoItem> GetToDoItemAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            try
            {
                var container = _cosmosClient.GetContainer(Constants.CosmosDb.DatabaseId, Constants.CosmosDb.Collection.ToDoItemCollection);
                var response = await container.ReadItemAsync<ToDoItem>(id, new PartitionKey("id"));
                var cost = response.RequestCharge;    // should never be more than 1 RU for documents under 1K

                _logger.LogWarning("Point read executed for item type ToDoItem total RU cost {RequestCharge}", response.RequestCharge);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetToDoItemAsync error");
                throw;
            }
        }

        public async Task<ToDoItem> UpsertToDoItemAsync(ToDoItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            try
            {
                var container = _cosmosClient.GetContainer(Constants.CosmosDb.DatabaseId, Constants.CosmosDb.Collection.ToDoItemCollection);
                var response = await container.UpsertItemAsync(item, new PartitionKey(item.Id));
                var cost = response.RequestCharge;    

                _logger.LogWarning("Upserted item type ToDoItem total RU cost {RequestCharge}", response.RequestCharge);

                return response.Resource;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error upserting ToDoItem for id {item.Id}");
            }
            return null;
        }

        public async Task<ToDoItem> DeleteToDoItemAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            try
            {
                var container = _cosmosClient.GetContainer(Constants.CosmosDb.DatabaseId, Constants.CosmosDb.Collection.ToDoItemCollection);                
                var response = await container.PatchItemAsync<ToDoItem>(id, new PartitionKey(id), patchOperations: new[] 
                {
                    PatchOperation.Replace("/ttl", 1)
                });
                var cost = response.RequestCharge;

                _logger.LogWarning("Partial update to delete item type ToDoItem total RU cost {RequestCharge}", response.RequestCharge);

                return response.Resource;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error doing partial update on ToDoItem for id {id}");
            }
            return null;
        }
    }
}
