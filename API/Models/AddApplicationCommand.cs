using System.Text.Json.Serialization;

namespace WebApplication.Models
{
    public class AddApplicationCommand
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }
        
        [JsonPropertyName("department_address")]
        public string DepartmentAddress { get; set; }
        
        public decimal Amount { get; set; }
        
        public string Currency { get; set; }
    }
}