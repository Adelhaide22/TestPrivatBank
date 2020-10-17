using System.Text.Json.Serialization;

namespace WebApplication.Models
{
    public class GetApplicationByClientIdQuery
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }
        
        [JsonPropertyName("department_address")]
        public string DepartmentAddress { get; set; }
    }
}