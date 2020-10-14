using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Contracts;
using Dapper;

namespace Worker
{
    public class Repository : IRepository
    {
        private IDbConnection db;
        public Repository(IDbConnection connection)
        {
            db = connection;
        }
        public void AddApplication(AddApplicationMqCommand command)
        {
            var sqlQuery = "  "; // SP
            db.Execute(sqlQuery, command);
        }

        public Application GetApplicationByRequestId(GetApplicationByRequestIdMqCommand command)
        {
            return db.Query<Application>("  ").FirstOrDefault(); // SP
        }

        public Application GetApplicationByClientId(GetApplicationByClientIdMqCommand command)
        {
            return db.Query<Application>("  ").FirstOrDefault(); // SP
        }
    }
}