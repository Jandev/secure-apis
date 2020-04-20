using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SecureApi.Api.Infrastructure.Health
{
    public class WarmupService : IHostedService
    {
        private readonly SpeakerService speakerService;
        private readonly ILogger<WarmupService> logger;

        public WarmupService(
            SpeakerService speakerService,
            ILogger<WarmupService> logger)
        {
            this.speakerService = speakerService;
            this.logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Executing {nameof(WarmupService)}.");
            await this.speakerService.CheckService(cancellationToken);
            this.speakerService.StartupTaskCompleted = true;
            this.logger.LogInformation($"Executed {nameof(WarmupService)}.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class SpeakerService : IHealthCheck
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<SpeakerService> logger;

        private volatile bool startupTaskCompleted = false;

        internal bool StartupTaskCompleted
        {
            get => startupTaskCompleted;
            set => startupTaskCompleted = value;
        }

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

            try
            {
                if (StartupTaskCompleted)
                {
                    return HealthCheckResult.Healthy("Already warmed up.");
                }
                var response = await CheckService(cancellationToken);
                
                logger.LogInformation($"Executed health check for {nameof(SpeakerService)}.");
                if ((response.StatusCode == HttpStatusCode.Unauthorized ||
                     response.StatusCode == HttpStatusCode.Forbidden) &&
                    !"Site Disabled".Equals(response.ReasonPhrase, StringComparison.InvariantCultureIgnoreCase))
                {
                    return HealthCheckResult.Healthy("Got expected response from Speaker Api.");
                }
                return HealthCheckResult.Unhealthy($"Got status code {response.StatusCode} with reason `{response.ReasonPhrase}`.");
            }
            catch (HttpRequestException httpRequestException)
            {
                return HealthCheckResult.Unhealthy(httpRequestException.Message);
            }
        }

        internal async Task<HttpResponseMessage> CheckService(CancellationToken cancellationToken = new CancellationToken())
        {
            string speakerApiUri = this.configuration["SpeakerApiUri"];

            var httpClient = this.clientFactory.CreateClient(nameof(SpeakerService));

            try
            {
                var response = await httpClient.GetAsync(speakerApiUri, cancellationToken);
                return response;
            }
            catch (HttpRequestException httpRequestException)
            {
                logger.LogWarning(httpRequestException, $"The {nameof(SpeakerService)} health check failed.");
                throw;
            }
        }
    }
}
