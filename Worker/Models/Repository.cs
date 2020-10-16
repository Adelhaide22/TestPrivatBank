using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Contracts;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Worker
{
    public class Repository : IRepository
    {
        private readonly IDbConnection db;
        private readonly ILogger<Repository> _logger;

        public Repository(IDbConnection connection, ILogger<Repository> logger)
        {
            db = connection;
            _logger = logger;
        }
        public int AddApplication(AddApplicationMqCommand command)
        {
            var p = new DynamicParameters();
            p.Add("@ClientId", command.ClientId, DbType.Int32);
            p.Add("@DepartmentAddress", command.DepartmentAddress, DbType.String);
            p.Add("@Amount", command.Amount, DbType.Decimal);
            p.Add("@Currency", command.Currency, DbType.String);
            p.Add("@State", "Ready", DbType.String);
            
            try
            {
                return db.Query<int>("AddApplication", p, commandType: CommandType.StoredProcedure).Single();
            }
            catch (SqlException e)
            {
                _logger.LogError($"Error in database: {e.Message}");
                throw;
            }
        }

        public IList<object> GetApplicationsByRequestId(GetApplicationByRequestIdMqCommand command)
        { 
            try
            {
                return db.Query<Application>("GetApplicationByRequestId", new {command.RequestId}, commandType: CommandType.StoredProcedure)
                .Select(a => (object)new
                {
                    Amount = a.Amount, 
                    Currency = a.Currency, 
                    State = a.State
                })
                .ToList();
            }
            catch (SqlException e)
            {
                _logger.LogError($"Error in database: {e.Message}");
                throw;
            }
        }

        public IList<object> GetApplicationsByClientId(GetApplicationByClientIdMqCommand command)
        {
            try
            {
                return db.Query<Application>("GetApplicationByClientId", new {command.ClientId, command.DepartmentAddress}, 
                commandType: CommandType.StoredProcedure)
                .Select(a => (object)new
                {
                    Amount = a.Amount, 
                    Currency = a.Currency, 
                    State = a.State
                })
                .ToList();
            }
            catch (SqlException e)
            {
                _logger.LogError($"Error in database: {e.Message}");
                throw;
            }
        }
    }
}