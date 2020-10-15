using System.Text.Json.Serialization;

namespace WebApplication
{
    public class GetApplicationByRequestIdCommand
    {
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }
    }
}