using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureApi.Domain.Contracts.Model;

namespace SecureApi.Speaker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpeakersController : ControllerBase
    {
        private readonly ILogger<SpeakersController> _logger;
        private List<Domain.Contracts.Model.Speaker> speakers;

        public SpeakersController(ILogger<SpeakersController> logger)
        {
            _logger = logger;
            InitializeSpeakers();
        }

        private void InitializeSpeakers()
        {
            speakers = new List<Domain.Contracts.Model.Speaker>();
            speakers.AddRange(new[]
            {
                new Domain.Contracts.Model.Speaker
                {
                    FirstName = "Jan",
                    LastName = "de Vries",
                    Level = Level.Moderate
                },
                new Domain.Contracts.Model.Speaker
                {
                    FirstName = "Public",
                    LastName = "the Speaker",
                    Level = Level.Keynote
                },
                new Domain.Contracts.Model.Speaker
                {
                    FirstName = "Anon",
                    LastName = "Ymous",
                    Level = Level.Beginner
                }
            });
        }

        [HttpGet]
        [Authorize(Roles = "SecureApi.Speaker.Reader")]
        public IEnumerable<Domain.Contracts.Model.Speaker> Get()
        {
            return this.speakers;
        }
    }
}
