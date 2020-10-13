using Contracts;

namespace QueryHandler
{
    public interface IRepository
    {
        void AddApplication(AddApplicationMqCommand command);
        GetApplicationByRequestIdMqCommand GetApplicationByRequestId(GetApplicationByRequestIdMqCommand command);
        GetApplicationByClientIdMqCommand GetApplicationByClientId(GetApplicationByClientIdMqCommand command);
    }
}