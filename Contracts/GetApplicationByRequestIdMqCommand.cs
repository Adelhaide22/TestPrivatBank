namespace Contracts
{
    public class GetApplicationByRequestIdMqCommand : ICommand
    {
        public string RequestId { get; set; }
        
        public string ClientIp { get; set; }
    }
}