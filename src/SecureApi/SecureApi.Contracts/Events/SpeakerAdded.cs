using System;

namespace SecureApi.Domain.Contracts.Events
{
    public class SpeakerAdded
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}