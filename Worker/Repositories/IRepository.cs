using System.Collections.Generic;
using Contracts;
using Worker.Models;

namespace Worker.Repositories
{
    public interface IRepository
    {
        int AddApplication(AddApplicationMqCommand command);
        IList<ApplicationModel> GetApplicationsByRequestId(GetApplicationByRequestIdMqQuery command);
        IList<ApplicationModel> GetApplicationsByClientId(GetApplicationByClientIdMqQuery command);
    }
}