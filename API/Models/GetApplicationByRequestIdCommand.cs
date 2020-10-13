using Microsoft.AspNetCore.Mvc;

namespace WebApplication
{
    public class GetApplicationByRequestIdCommand
    {
        [BindProperty(Name = "request_id")]
        public string RequestId { get; set; }
    }
}