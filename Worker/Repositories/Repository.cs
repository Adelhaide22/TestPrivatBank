using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Contracts;
using Dapper;
using Microsoft.Extensions.Logging;
using Worker.Models;

namespace Worker.Repositories
{
    public class Repository : IRepository
    {
        private readonly IDbConnection _db;
        private readonly ILogger<Repository> _logger;

        public Repository(IDbConnection connection, ILogger<Repository> logger)
        {
            _db = connection;
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
                return _db.Query<int>("AddApplication", p, commandType: CommandType.StoredProcedure).Single();
            }
            catch (SqlException e)
            {
                _logger.LogError($"Error in database: {e.Message}");
                throw;
            }
        }

        public IList<ApplicationModel> GetApplicationsByRequestId(GetApplicationByRequestIdMqQuery command)
        { 
            try
            {
                return _db.Query<ApplicationModel>("GetApplicationByRequestId", new {command.RequestId}, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (SqlException e)
            {
                _logger.LogError($"Error in database: {e.Message}");
                throw;
            }
        }

        public IList<ApplicationModel> GetApplicationsByClientId(GetApplicationByClientIdMqQuery command)
        {
            try
            {
                return _db.Query<ApplicationModel>("GetApplicationByClientId", new {command.ClientId, command.DepartmentAddress}, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (SqlException e)
            {
                _logger.LogError($"Error in database: {e.Message}");
                throw;
            }
        }
    }
}