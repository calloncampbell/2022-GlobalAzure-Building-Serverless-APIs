using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using TodoListApi.FunctionApp.Abstractions.Constants;

namespace TodoListApi.FunctionApp.Functions.HttpTrigger
{
    public class Welcome
    {
        private static IConfigurationRoot _configuration { set; get; }
        private readonly IConfigurationRefresher _configurationRefresher;
        private readonly IFeatureManager _featureManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Welcome" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="configurationRefresher">The configuration refresher.</param>
        /// <param name="featureManager">The feature manager.</param>
        public Welcome(
            IConfigurationRoot configuration,
            IConfigurationRefresher configurationRefresher,
            IFeatureManager featureManager)
        {
            _configuration = configuration;
            _configurationRefresher = configurationRefresher;
            _featureManager = featureManager;
        }

        [FunctionName(nameof(Welcome))]
        [OpenApiOperation(operationId: nameof(Welcome))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("HTTP trigger function processed a request for Welcome");

            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var functionRuntimeVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
            var regionName = Environment.GetEnvironmentVariable("REGION_NAME");

            await _configurationRefresher.TryRefreshAsync();

            var sentinel = _configuration[$"{Constants.AppConfig.AppPrefix}:Sentinel"];

            var responseMessage = new
            {
                ApplicationName = appName,
                ApplicationVersion = appVersion,
                Region = regionName,
                FunctionRuntimeVersion = functionRuntimeVersion,
                ConfigurationSentinel = sentinel,
                CurrentDatetime = DateTime.UtcNow,
            };

            return new OkObjectResult(responseMessage);
        }
    }
}

