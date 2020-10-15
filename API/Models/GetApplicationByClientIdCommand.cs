using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication
{
    public class GetApplicationByClientIdCommand
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }
        
        [JsonPropertyName("department_address")]
        public string DepartmentAddress { get; set; }
    }
}