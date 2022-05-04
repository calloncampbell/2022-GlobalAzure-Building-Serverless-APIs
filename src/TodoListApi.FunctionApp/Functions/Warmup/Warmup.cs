using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TodoListApi.FunctionApp.Functions.Warmup
{
    public static class Warmup
    {
        // Declare shared dependencies here

        /// <summary>
        /// Runs the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="log">The log.</param>
        [FunctionName(nameof(Warmup))]
        public static void Run(
            [WarmupTrigger()] WarmupContext context,
            ILogger log)
        {
            // Initialize shared dependencies here

            log.LogInformation("Function App instance is warm");
        }
    }
}
