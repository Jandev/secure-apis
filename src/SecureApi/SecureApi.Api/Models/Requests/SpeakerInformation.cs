using SecureApi.Domain.Contracts.Model;

namespace SecureApi.Api.Models.Requests
{
    public class SpeakerInformation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Level Level { get; set; } 
    }
}