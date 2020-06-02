using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SecureApi.Speaker.Worker
{
    public static class PickupSpeakerCommands
    {
        [FunctionName(nameof(PickupSpeakerCommands))]
        public static void Run(
            [QueueTrigger("speaker-commands", Connection = "Speakers:StorageAccount")]
            string myQueueItem, 
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
