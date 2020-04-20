using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SecureApi.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<WeatherForecastController> logger;

        public WeatherForecastController(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<WeatherForecastController> logger)
        {
            this.clientFactory = clientFactory;
            this.configuration = configuration;
            this.logger = logger;
        }

        public class ApiCallDetails
        {
            public string AccessToken { get; set; }
            public string Body { get; set; }
            public int StatusCode { get; set; }
            public string Reason { get; set; }
        }
        [HttpGet]
        public async Task<ApiCallDetails> Get()
        {
            this.logger.LogInformation($"Executing {nameof(Get)}.");
            string applicationIdUri = this.configuration["ApplicationIdUri"];
            string speakerApiUri = this.configuration["SpeakerApiUri"];
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var tenantId = this.configuration["ActiveDirectory:TenantId"];
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(applicationIdUri, tenantId: tenantId);

            var httpClient = this.clientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync(speakerApiUri);
            var body = await response.Content.ReadAsStringAsync();

            this.logger.LogInformation($"Executed {nameof(Get)}.");
            return new ApiCallDetails
            {
                AccessToken = accessToken,
                Body = body,
                StatusCode = (int)response.StatusCode,
                Reason = response.ReasonPhrase
            };
        }
    }
}
