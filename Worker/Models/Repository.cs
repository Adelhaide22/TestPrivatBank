using System.Collections.Generic;
using System.Data;
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
        public int AddApplication(AddApplicationMqCommand command)
        {
            var p = new DynamicParameters();
            p.Add("@ClientId", command.ClientId, DbType.Int32);
            p.Add("@DepartmentAddress", command.DepartmentAddress, DbType.String);
            p.Add("@Amount", command.Amount, DbType.Decimal);
            p.Add("@Currency", command.Currency, DbType.String);
            p.Add("@State", "Ready", DbType.String);
            
            return db.Query<int>("AddApplication", p, commandType: CommandType.StoredProcedure).Single();
        }

        public IList<(decimal Amount, string Currency, string State)> GetApplicationsByRequestId(GetApplicationByRequestIdMqCommand command)
        {
            return db.Query<Application>("GetApplicationByRequestId", new {command.RequestId}, 
                commandType: CommandType.StoredProcedure)
                .Select(a => (a.Amount, a.Currency, a.State))
                .ToList();
        }

        public IList<(decimal Amount, string Currency, string State)> GetApplicationsByClientId(GetApplicationByClientIdMqCommand command)
        {
            return db.Query<Application>("GetApplicationByClientId", new {command.ClientId, command.DepartmentAddress}, 
                commandType: CommandType.StoredProcedure)
                .Select(a => (a.Amount, a.Currency, a.State))
                .ToList();
        }
    }
}