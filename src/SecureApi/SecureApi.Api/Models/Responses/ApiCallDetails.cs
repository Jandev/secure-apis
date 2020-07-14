namespace SecureApi.Api.Models.Responses
{
    public class ApiCallDetails
    {
        public string AccessToken { get; set; }
        public string Body { get; set; }
        public int StatusCode { get; set; }
        public string Reason { get; set; }
    }
}
