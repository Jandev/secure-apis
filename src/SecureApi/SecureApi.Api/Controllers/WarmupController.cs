using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Get()
        {
            this.logger.LogInformation($"Executing {nameof(WarmupController)}.{nameof(Get)}");

            await Task.Delay(5000);

            this.logger.LogInformation($"Executed {nameof(WarmupController)}.{nameof(Get)}");
            return Ok();
        }
    }
}