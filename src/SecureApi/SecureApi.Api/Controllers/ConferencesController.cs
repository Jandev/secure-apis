using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecureApi.Api.Models.Responses;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;

namespace SecureApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConferencesController : ControllerBase
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<ConferencesController> logger;

        public ConferencesController(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<ConferencesController> logger)
        {
            this.clientFactory = clientFactory;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ApiCallDetails> GetConferences()
        {
            this.logger.LogInformation($"Executing {nameof(GetConferences)}.");

            var response = await InvokeConferencesService();

            this.logger.LogInformation($"Executed {nameof(GetConferences)}.");

            return response;
        }

        private async Task<ApiCallDetails> InvokeConferencesService()
        {
            string speakerApiUri = this.configuration["Conferences:ConferencesApiUri"];

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

        public async Task<string> GenerateAccessToken()
        {
            string applicationIdUri = this.configuration["Conferences:ApplicationIdUri"];
            var tenantId = this.configuration["ActiveDirectory:TenantId"];

            // Create an access token using the Managed Identity of this application.
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
