using AzureFunctions.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SecureApi.Domain.Contracts.Commands;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecureApi.Speaker.Worker
{
    public class PickupSpeakerCommands
    {
        [FunctionName(nameof(PickupSpeakerCommands))]
        public async Task Run(
            [QueueTrigger("speaker-commands", Connection = "Speakers:StorageAccount")]
            string command,
            [EventGrid(
                // The endpoint of your Event Grid Topic, this should be specified in your application settings of the Function App
                TopicEndpoint = "EventGridBindingSampleTopicEndpoint", 
                // This is the secret key to connect to your Event Grid Topic. To be placed in the application settings.
                TopicKey = "EventGridBindingSampleTopicKey")]
            IAsyncCollector<Event> outputCollector,
            ILogger log)
        {
            log.LogInformation($"Executing {nameof(PickupSpeakerCommands)}");

            var deserializedCommand = JsonSerializer.Deserialize<AddSpeaker>(command);

            // Domain logic over here...

            await EmitEvent(outputCollector, deserializedCommand);
        }

        private static async Task EmitEvent(IAsyncCollector<Event> outputCollector, AddSpeaker deserializedCommand)
        {
            // Specify some meta data of the message you want to publish to Event Grid
            var myTestEvent = new Event
            {
                EventType = "SecureApi.Speaker.SpeakerAdded",
                Subject = "New Speaker",
                Data = new SpeakerAdded
                {
                    Id = deserializedCommand.Id,
                    FirstName = deserializedCommand.FirstName,
                    LastName = deserializedCommand.LastName
                },
                DataVersion = "1.0"
            };

            // Add the event to the IAsyncCollector<T> in order to get your event published.
            await outputCollector.AddAsync(myTestEvent);
        }
    }
}
