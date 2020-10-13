using Microsoft.AspNetCore.Mvc;

namespace WebApplication
{
    public class GetApplicationByClientIdCommand
    {
        [BindProperty(Name = "client_id")]
        public string ClientId { get; set; }

        [BindProperty(Name = "department_address")]
        public string DepartmentAddress { get; set; }
    }
}