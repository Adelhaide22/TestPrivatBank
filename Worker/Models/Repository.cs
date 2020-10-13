using Contracts;

namespace QueryHandler
{
    public class Repository : IRepository
    {
        public void AddApplication(AddApplicationMqCommand command)
        {
            throw new System.NotImplementedException();
        }

        public GetApplicationByRequestIdMqCommand GetApplicationByRequestId(GetApplicationByRequestIdMqCommand command)
        {
            throw new System.NotImplementedException();
        }

        public GetApplicationByClientIdMqCommand GetApplicationByClientId(GetApplicationByClientIdMqCommand command)
        {
            throw new System.NotImplementedException();
        }
    }
}