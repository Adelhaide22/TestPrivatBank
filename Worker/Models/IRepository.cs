using System.Collections.Generic;
using Contracts;

namespace Worker
{
    public interface IRepository
    {
        int AddApplication(AddApplicationMqCommand command);
        IList<ApplicationModel> GetApplicationsByRequestId(GetApplicationByRequestIdMqCommand command);
        IList<ApplicationModel> GetApplicationsByClientId(GetApplicationByClientIdMqCommand command);
    }
}