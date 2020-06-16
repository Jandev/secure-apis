using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SecureApi.Domain.Contracts.Commands;
using SecureApi.Domain.Contracts.Model;

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
        public async Task<IActionResult> Add(SpeakerInformation speakerInformation, CancellationToken cancellationToken)
        {
            var command = new AddSpeaker
            {
                Id = Guid.NewGuid(),
                FirstName = speakerInformation.FirstName,
                LastName = speakerInformation.LastName,
                Level = speakerInformation.Level
            };

            // Send the command to the queue
            await SendToQueue(command, cancellationToken);

            return Accepted(command.Id);
        }

        private async Task SendToQueue(AddSpeaker command, CancellationToken cancellationToken)
        {
            // Get the connection string from app settings
            string connectionString = this.configuration["Speakers:StorageAccount"];
            string queueName = this.configuration["Speakers:CommandQueueName"];

            var serializedCommand = JsonSerializer.Serialize(command);
            
            var queueClient = new QueueClient(connectionString, queueName);
            
            if (await queueClient.ExistsAsync(cancellationToken))
            {
                await queueClient.SendMessageAsync(
                    Base64Encode(serializedCommand), 
                    cancellationToken);
            }
        }

        /// <remarks>
        /// Need base64 encoded messages in the latest package:
        /// https://github.com/Azure/azure-sdk-for-net/issues/10242
        /// </remarks>>
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }

    public class SpeakerInformation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Level Level { get; set; } 
    }
}