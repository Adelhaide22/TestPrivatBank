namespace Contracts
{
    public class AddApplicationMqCommand : ICommand
    {
        public string ClientId { get; set; }
        
        public string ClientIp { get; set; }

        public string DepartmentAddress { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }
    } 
}