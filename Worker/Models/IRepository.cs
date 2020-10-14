using System.Collections.Generic;
using Contracts;

namespace Worker
{
    public interface IRepository
    {
        int AddApplication(AddApplicationMqCommand command);
        IList<Application> GetApplicationsByRequestId(GetApplicationByRequestIdMqCommand command);
        IList<Application> GetApplicationsByClientId(GetApplicationByClientIdMqCommand command);
    }
}