using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SpeakerService> _logger;

        public SpeakerService(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            ILogger<SpeakerService> logger)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _logger = logger;
        }
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            _logger.LogInformation($"Executing health check for {nameof(SpeakerService)}.");



            _logger.LogInformation($"Executed health check for {nameof(SpeakerService)}.");
        }
    }
}
