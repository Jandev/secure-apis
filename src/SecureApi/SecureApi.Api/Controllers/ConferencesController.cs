using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecureApi.Api.Models.Responses;
using System.Net.Http;
using System.Threading.Tasks;

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
            string speakerApiUri = this.configuration["ConferencesApiUri"];

            var httpClient = this.clientFactory.CreateClient();
            var response = await httpClient.GetAsync(speakerApiUri);
            var body = await response.Content.ReadAsStringAsync();

            var callDetails = new ApiCallDetails
            {
                Body = body,
                StatusCode = (int)response.StatusCode,
                Reason = response.ReasonPhrase
            };

            return callDetails;
        }
    }
}
