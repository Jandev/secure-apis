using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SecureApi.Api.Worker
{
    public static class ProcessAddedSpeaker
    {
        [FunctionName(nameof(ProcessAddedSpeaker))]
        public static void Run(
            [QueueTrigger("added-speakers", Connection = "Events:StorageAccount")]
            string forwardedEvent, 
            ILogger log)
        {
            log.LogInformation($"Executing {nameof(ProcessAddedSpeaker)} for event {forwardedEvent}");

            var eventGridEvent = JsonConvert.DeserializeObject<EventGridEvent>(forwardedEvent);

            // Domain logic over here...
        }
    }
}
