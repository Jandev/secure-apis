using System;

namespace SecureApi.Speaker.Worker
{
    public class SpeakerAdded
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}