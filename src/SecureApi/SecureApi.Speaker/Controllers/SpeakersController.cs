using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureApi.Speaker.Model;

namespace SecureApi.Speaker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpeakersController : ControllerBase
    {
        private readonly ILogger<SpeakersController> _logger;
        private List<Model.Speaker> speakers;

        public SpeakersController(ILogger<SpeakersController> logger)
        {
            _logger = logger;
            InitializeSpeakers();
        }

        private void InitializeSpeakers()
        {
            speakers = new List<Model.Speaker>();
            speakers.AddRange(new[]
            {
                new Model.Speaker
                {
                    FirstName = "Jan",
                    LastName = "de Vries",
                    Level = Level.Moderate
                },
                new Model.Speaker
                {
                    FirstName = "Public",
                    LastName = "the Speaker",
                    Level = Level.Keynote
                },
                new Model.Speaker
                {
                    FirstName = "Anon",
                    LastName = "Ymous",
                    Level = Level.Beginner
                }
            });
        }

        [HttpGet]
        [Authorize(Roles = "SecureApi.Speaker.Reader")]
        public IEnumerable<Model.Speaker> Get()
        {
            return this.speakers;
        }
    }
}
