namespace Contracts
{
    public class GetApplicationByRequestIdMqCommand
    {
        public string RequestId { get; set; }
        
        public string ClientIp { get; set; }
    }
}