using System.Collections.Generic;
using Contracts;

namespace Worker
{
    public interface IRepository
    {
        int AddApplication(AddApplicationMqCommand command);
        IList<(decimal Amount, string Currency, string State)> GetApplicationsByRequestId(GetApplicationByRequestIdMqCommand command);
        IList<(decimal Amount, string Currency, string State)> GetApplicationsByClientId(GetApplicationByClientIdMqCommand command);
    }
}