using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace SecureApi.Conferences.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BigConferencesController : ControllerBase
    {
        private readonly ILogger<BigConferencesController> logger;

        public BigConferencesController(ILogger<BigConferencesController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public IEnumerable<Conference> Get()
        {
            var conferences = new List<Conference>();

            conferences.AddRange(new []{
                new Conference
                {
                    Id = Guid.NewGuid(),
                    Name = "NDC Oslo"
                },
                new Conference
                {
                    Id = Guid.NewGuid(),
                    Name = "NDC Sydney"
                },
                new Conference
                {
                    Id = Guid.NewGuid(),
                    Name = "Techorama"
                }});

            return conferences;
        }
    }

    public class Conference
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
