using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace SecureApi.Api.Infrastructure.Health
{
    public class SpeakerService : IHealthCheck
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<SpeakerService> logger;

        public SpeakerService(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<SpeakerService> logger)
        {
            this.clientFactory = clientFactory;
            this.configuration = configuration;
            this.logger = logger;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            logger.LogInformation($"Executing health check for {nameof(SpeakerService)}.");

            string speakerApiUri = this.configuration["SpeakerApiUri"];

            var httpClient = this.clientFactory.CreateClient(nameof(SpeakerService));

            try
            {
                var response = await httpClient.GetAsync(speakerApiUri, cancellationToken);
                logger.LogInformation($"Executed health check for {nameof(SpeakerService)}.");
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return HealthCheckResult.Healthy("Got expected response from Speaker Api.");
                }

                return HealthCheckResult.Unhealthy($"Got status code {response.StatusCode}.");
            }
            catch (HttpRequestException httpRequestException)
            {
                logger.LogWarning(httpRequestException, $"The {nameof(SpeakerService)} health check failed.");
                return HealthCheckResult.Unhealthy(httpRequestException.Message);
            }
        }
    }
}
