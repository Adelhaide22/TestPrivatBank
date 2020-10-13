namespace Contracts
{
    public class GetApplicationByClientIdMqCommand
    {
        public string ClientId { get; set; }
        
        public string ClientIp { get; set; }
        
        public string DepartmentAddress { get; set; }
    }
}