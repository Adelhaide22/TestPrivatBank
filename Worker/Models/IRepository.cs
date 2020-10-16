using System.Collections.Generic;
using Contracts;

namespace Worker
{
    public interface IRepository
    {
        int AddApplication(AddApplicationMqCommand command);
        IList<object> GetApplicationsByRequestId(GetApplicationByRequestIdMqCommand command);
        IList<object> GetApplicationsByClientId(GetApplicationByClientIdMqCommand command);
    }
}