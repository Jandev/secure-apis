using System;
using SecureApi.Speaker.Model;

namespace SecureApi.Domain.Contracts.Commands
{
    public class AddSpeaker
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Level Level { get; set; }
    }
}
