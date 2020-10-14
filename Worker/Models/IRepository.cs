using Contracts;

namespace Worker
{
    public interface IRepository
    {
        void AddApplication(AddApplicationMqCommand command);
        Application GetApplicationByRequestId(GetApplicationByRequestIdMqCommand command);
        Application GetApplicationByClientId(GetApplicationByClientIdMqCommand command);
    }
}