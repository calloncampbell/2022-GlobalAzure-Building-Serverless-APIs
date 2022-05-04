// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Messaging.EventGrid;

namespace TodoListApi.FunctionApp.Functions.EventGridTrigger
{
    public class ConfigurationChangeEvent
    {
        private IConfigurationRefresher _configurationRefresher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationChangeEvent"/> class.
        /// </summary>
        /// <param name="configurationRefresher">The configuration refresher.</param>
        public ConfigurationChangeEvent(IConfigurationRefresher configurationRefresher)
        {
            _configurationRefresher = configurationRefresher;
        }

        /// <summary>
        /// Runs the specified event grid event.
        /// </summary>
        /// <param name="eventGridEvent">The event grid event.</param>
        /// <param name="log">The log.</param>
        [FunctionName(nameof(ConfigurationChangeEvent))]
        public void Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation(eventGridEvent.Data.ToString());

            // A random delay is added before the cached value is marked as dirty to reduce potential
            // throttling in case multiple instances refresh at the same time.
            _configurationRefresher.SetDirty();
        }
    }
}
