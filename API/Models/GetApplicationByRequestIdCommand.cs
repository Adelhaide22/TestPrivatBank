using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication
{
    public class GetApplicationByRequestIdCommand
    {
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }
    }
}