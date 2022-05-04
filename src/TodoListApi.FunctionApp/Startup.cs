using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoListApi.FunctionApp.Abstractions.Interfaces;
using TodoListApi.FunctionApp.Services;
using Constants = TodoListApi.FunctionApp.Abstractions.Constants.Constants;

[assembly: FunctionsStartup(typeof(TodoListApi.FunctionApp.Startup))]
namespace TodoListApi.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        private static IConfigurationRoot Configuration { get; set; }
        public IConfigurationBuilder ConfigurationBuilder { get; set; }
        private static IConfigurationRefresher ConfigurationRefresher { set; get; }

        public Startup()
        {
            ConfigurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddEnvironmentVariables();

            Configuration = ConfigurationBuilder.Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var regionName = Environment.GetEnvironmentVariable("REGION_NAME");
            if (string.IsNullOrWhiteSpace(regionName))
            {
                regionName = Regions.CanadaCentral;
            }

            // Load configuration from Azure App Configuration (Primary or Secondary Stores)
            var appConfigEndpoint_PrimaryStore = Environment.GetEnvironmentVariable("AzureAppConfiguration.Endpoint_PrimaryStore");
            var appConfigEndpoint_SecondaryStore = Environment.GetEnvironmentVariable("AzureAppConfiguration.Endpoint_SecondaryStore");
            var cacheExpiryInSeconds = double.Parse(Environment.GetEnvironmentVariable("AzureAppConfiguration.CacheExpirationTimeInSeconds") ?? "300");
            var environmentLabel = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AzureAppConfiguration.EnvironmentLabel"))
                ? Environment.GetEnvironmentVariable("AzureAppConfiguration.EnvironmentLabel")
                : LabelFilter.Null;

            var defaultAzureCredential = new DefaultAzureCredential();

            ConfigurationBuilder
                .AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(appConfigEndpoint_SecondaryStore), defaultAzureCredential)
                           .Select($"{Constants.AppConfig.AppPrefix}:*")
                           .Select($"{Constants.AppConfig.AppPrefix}:*", environmentLabel)                           
                           .ConfigureRefresh(refreshOptions =>
                                refreshOptions.Register(key: $"{Constants.AppConfig.AppPrefix}:Sentinel", label: environmentLabel, refreshAll: true)
                                              .SetCacheExpiration(TimeSpan.FromSeconds(cacheExpiryInSeconds))
                           )
                           .UseFeatureFlags(flagOptions =>
                           {
                               flagOptions.Label = environmentLabel;
                               flagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(cacheExpiryInSeconds);
                           })
                           .ConfigureKeyVault(kv =>
                           {
                               kv.SetCredential(defaultAzureCredential);
                           });
                    ConfigurationRefresher = options.GetRefresher();
                }, optional: true)
                .AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(appConfigEndpoint_PrimaryStore), defaultAzureCredential)
                           .Select($"{Constants.AppConfig.AppPrefix}:*")
                           .Select($"{Constants.AppConfig.AppPrefix}:*", environmentLabel)                           
                           .ConfigureRefresh(refreshOptions =>
                                refreshOptions.Register(key: $"{Constants.AppConfig.AppPrefix}:Sentinel", label: environmentLabel, refreshAll: true)
                                              .SetCacheExpiration(TimeSpan.FromSeconds(cacheExpiryInSeconds))
                           )
                           .UseFeatureFlags(flagOptions =>
                           {
                               flagOptions.Label = environmentLabel;
                               flagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(cacheExpiryInSeconds);
                           })
                           .ConfigureKeyVault(kv =>
                           {
                               kv.SetCredential(defaultAzureCredential);
                           });
                    ConfigurationRefresher = options.GetRefresher();
                }, optional: true);

            Configuration = ConfigurationBuilder.Build();

            // Register the CosmosClient as a Singleton
            var cosmosString = Configuration[Constants.CosmosDb.Connection];
            builder.Services.AddSingleton((s) =>
            {
                CosmosClientBuilder configurationBuilder = new CosmosClientBuilder(Configuration[Constants.CosmosDb.Connection])
                    .WithApplicationRegion(regionName);

                return configurationBuilder.Build();
            });

            // Post Configure used since DefaultCredentials is not supported in cosmos DB triggers yet
            builder.Services.PostConfigure<CosmosDBOptions>(options =>
            {
                options.ConnectionString = Configuration[Constants.CosmosDb.Connection];
            });

            builder.Services.AddLogging();
            builder.Services.AddSingleton(Configuration);
            builder.Services.AddSingleton(ConfigurationRefresher);
            builder.Services.AddFeatureManagement(Configuration);

            builder.Services.AddScoped<IToDoListService, ToDoListService>();
        }
    }
}
