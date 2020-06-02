using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SecureApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarmupController : ControllerBase
    {
        private readonly ILogger<WarmupController> logger;

        public WarmupController(ILogger<WarmupController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            this.logger.LogInformation($"Executing {nameof(WarmupController)}.{nameof(Get)}");

            this.logger.LogInformation($"Executed {nameof(WarmupController)}.{nameof(Get)}");
            return Ok();
        }
    }
}