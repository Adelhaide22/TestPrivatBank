using System.Text.Json.Serialization;

namespace WebApplication.Models
{
    public class GetApplicationByRequestIdQuery
    {
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }
    }
}