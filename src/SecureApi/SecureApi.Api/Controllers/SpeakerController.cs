using Azure.Storage.Queues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecureApi.Api.Models.Requests;
using SecureApi.Api.Models.Responses;
using SecureApi.Domain.Contracts.Commands;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SecureApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpeakerController : ControllerBase
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<SpeakerController> logger;

        public SpeakerController(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<SpeakerController> logger)
        {
            this.clientFactory = clientFactory;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ApiCallDetails> Get()
        {
            this.logger.LogInformation($"Executing {nameof(Get)}.");

            var response = await GetSpeakers();

            this.logger.LogInformation($"Executed {nameof(Get)}.");

            return response;
        }

        [HttpPost]
        public async Task<IActionResult> Add(SpeakerInformation speakerInformation, CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Executing {nameof(Add)}.");
            var command = new AddSpeaker
            {
                Id = Guid.NewGuid(),
                FirstName = speakerInformation.FirstName,
                LastName = speakerInformation.LastName,
                Level = speakerInformation.Level
            };

            // Send the command to the queue
            await SendToQueue(command, cancellationToken);

            this.logger.LogInformation($"Executed {nameof(Add)}.");
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
        /// </remarks>
        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private async Task<ApiCallDetails> GetSpeakers()
        {
            string speakerApiUri = this.configuration["SpeakerApiUri"];

            var accessToken = await GenerateAccessToken();

            var httpClient = this.clientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync(speakerApiUri);
            var body = await response.Content.ReadAsStringAsync();

            var callDetails = new ApiCallDetails
            {
                AccessToken = accessToken,
                Body = body,
                StatusCode = (int)response.StatusCode,
                Reason = response.ReasonPhrase
            };

            return callDetails;
        }

        private async Task<string> GenerateAccessToken()
        {
            string applicationIdUri = this.configuration["ApplicationIdUri"];
            var tenantId = this.configuration["ActiveDirectory:TenantId"];

            // Create an access token useing the Managed Identity of this application.
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider
                .GetAccessTokenAsync(
                    // This one is specified in the App Registration of the app you're making a call to
                    applicationIdUri,
                    // Useful for local debugging if your account lives in multiple tenants
                    tenantId);
            return accessToken;
        }

    }
}