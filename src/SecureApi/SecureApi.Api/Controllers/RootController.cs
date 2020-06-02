using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SecureApi.Api.Controllers
{
    [Route("/")]
    [ApiController]
    public class RootController : ControllerBase
    {
        private readonly ILogger<RootController> logger;

        public RootController(ILogger<RootController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            this.logger.LogInformation($"Executing {nameof(RootController)}.{nameof(Get)}");

            this.logger.LogInformation($"Executed {nameof(RootController)}.{nameof(Get)}");
            return Ok();
        }
    }
}