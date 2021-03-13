using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace GraphqlToTsql.Database
{
    /// <summary>
    /// Execute a SQL query
    /// </summary>
    internal interface IDbAccess
    {
        /// <summary>
        /// Perform the database query
        /// </summary>
        /// <param name="tsql">SQL query</param>
        /// <param name="tsqlParameters">SQL parameters</param>
        /// <returns>Result object with the resulting json and execution time</returns>
        Task<DbResult> QueryAsync(string tsql, Dictionary<string, object> tsqlParameters);
    }

    internal class DbAccess : IDbAccess
    {
        private readonly string _connectionString;

        public DbAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<DbResult> QueryAsync(
            string tsql,
            Dictionary<string, object> tsqlParameters)
        {
            try
            {
                var dbResult = await PerformQueryAsync(tsql, tsqlParameters);
                return dbResult;
            }
            catch (Exception e)
            {
                return new DbResult { DbError = $"{e.GetType().Name}: {e.Message}" };
            }
        }

        private async Task<DbResult> PerformQueryAsync(
            string tsql,
            Dictionary<string, object> tsqlParameters)
        {
            var timedQuery = $@"
DECLARE @startTime DATETIME = GETDATE();

{tsql}

SELECT DATEDIFF(millisecond, @startTime, GETDATE()) AS databaseQueryTime;
";

            var dapperTsqlParameters = ConvertTsqlParameters(tsqlParameters);
            var parameters = new DynamicParameters(dapperTsqlParameters);

            using (var connection = new SqlConnection(_connectionString))
            using (var reader = await connection.QueryMultipleAsync(timedQuery, parameters))
            {
                // Dapper returns long strings in chunks
                var dataJsonSegments = await reader.ReadAsync<string>();
                var dataJson = string.Concat(dataJsonSegments);

                // Read the milliseconds spent to execute the query
                var databaseQueryTime = await reader.ReadFirstAsync<int>();

                return new DbResult
                {
                    DataJson = dataJson,
                    DatabaseQueryTime = databaseQueryTime
                };
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
