using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SecureApi.Domain.Contracts.Commands;
using SecureApi.Speaker.Model;

namespace SecureApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeakerController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public SpeakerController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        [HttpPost]
        public async Task<IActionResult> Add(SpeakerInformation speakerInformation)
        {
            var command = new AddSpeaker
            {
                Id = Guid.NewGuid(),
                FirstName = speakerInformation.FirstName,
                LastName = speakerInformation.LastName,
                Level = speakerInformation.Level
            };

            // Send the command to the queue
            await SendToQueue(command);

            return Accepted(command.Id);
        }

        private async Task SendToQueue(AddSpeaker command)
        {
            // Get the connection string from app settings
            string connectionString = this.configuration["Speakers:StorageAccount"];
            string queueName = this.configuration["Speakers:CommandQueueName"];

            // Instantiate a QueueClient which will be used to create and manipulate the queue
            var queueClient = new QueueClient(connectionString, queueName);

            await queueClient.SendMessageAsync(JsonSerializer.Serialize(command));
        }
    }

    public class SpeakerInformation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Level Level { get; set; } 
    }
}