using Microsoft.AspNetCore.Mvc;

namespace WebApplication
{
    public class AddApplicationCommand
    {
        [BindProperty(Name = "client_id")]
        public string ClientId { get; set; }

        [BindProperty(Name = "department_address")]
        public string DepartmentAddress { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }
    }
}