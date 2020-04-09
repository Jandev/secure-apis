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
        private readonly IConfiguration _configuration;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<WeatherForecastController> logger)
        {
            this.clientFactory = clientFactory;
            _configuration = configuration;
            _logger = logger;
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
            const string applicationIdUri = "api://6d7649c2-de4f-4ce4-83f5-422d4f6c5fe0";
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var tenantId = _configuration["ActiveDirectory:TenantId"];
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(applicationIdUri, tenantId: tenantId);
            var httpClient = clientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync("https://janv-secureapi-speakers.azurewebsites.net/WeatherForecast");
            var body = await response.Content.ReadAsStringAsync();

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
