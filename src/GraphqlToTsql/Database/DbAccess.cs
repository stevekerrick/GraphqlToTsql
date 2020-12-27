using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace GraphqlToTsql.Database
{
    public interface IDbAccess
    {
        Task<DbResult> QueryAsync(string tsql, Dictionary<string, object> tsqlParameters);
    }

    public class DbAccess : IDbAccess
    {
        private readonly string _connectionString;

        public DbAccess(
            IConnectionStringProvider connectionStringProvider)
        {
            _connectionString = connectionStringProvider.Get();
        }

        public async Task<DbResult> QueryAsync(
            string tsql,
            Dictionary<string, object> tsqlParameters)
        {
            try
            {
                var dataJson = await PerformQueryAsync(tsql, tsqlParameters);
                return new DbResult { DataJson = dataJson };
            }
            catch (Exception e)
            {
                return new DbResult { DbError = $"{e.GetType().Name}: {e.Message}" };
            }
        }

        private async Task<string> PerformQueryAsync(
            string tsql,
            Dictionary<string, object> tsqlParameters)
        {
            var dapperTsqlParameters = ConvertTsqlParameters(tsqlParameters);
            var parameters = new DynamicParameters(dapperTsqlParameters);

            using (var connection = new SqlConnection(_connectionString))
            {
                // Dapper returns long strings in chunks
                var dataJsonSegments = await connection.QueryAsync<string>(tsql, parameters);
                var dataJson = string.Concat(dataJsonSegments);
                return dataJson;
            }
        }

        private static Dictionary<string, object> ConvertTsqlParameters(Dictionary<string, object> tsqlParameters)
        {
            if (tsqlParameters == null)
            {
                tsqlParameters = new Dictionary<string, object>();
            }

            // Dapper wants the parameter names prepended with @
            var dapperTsqlParameters = new Dictionary<string, object>();
            foreach (var kv in tsqlParameters)
            {
                dapperTsqlParameters[$"@{kv.Key}"] = kv.Value;
            }

            return dapperTsqlParameters;
        }
    }
}
