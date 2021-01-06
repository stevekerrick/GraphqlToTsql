using Dapper;
using System;
using System.Collections;
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

        private static readonly (string, string)[] _statisticKeys = new[]
        {
            ("BytesSent", "Bytes sent to DB"),
            ("BytesReceived", "Bytes received from DB"),
            ("ConnectionTime", "DB connection time (ms)"),
            ("ExecutionTime", "DB execution time (ms)"),
            ("NetworkServerTime", "DB network server time (ms)")
        };

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
            var dapperTsqlParameters = ConvertTsqlParameters(tsqlParameters);
            var parameters = new DynamicParameters(dapperTsqlParameters);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.StatisticsEnabled = true;

                // Dapper returns long strings in chunks
                var dataJsonSegments = await connection.QueryAsync<string>(tsql, parameters);
                var dataJson = string.Concat(dataJsonSegments);

                var statistics = GetStatistics(connection.RetrieveStatistics());

                return new DbResult
                {
                    DataJson = dataJson,
                    Statistics = statistics
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

        private static List<Statistic> GetStatistics(IDictionary connectionStatistics)
        {
            var statistics = new List<Statistic>();

            foreach (var statisticKey in _statisticKeys)
            {
                var value = GetValue(connectionStatistics, statisticKey.Item1);
                statistics.Add(new Statistic(statisticKey.Item2, value));
            }

            return statistics;
        }

        private static long? GetValue(IDictionary connectionStatistics, string key)
        {
            if (!connectionStatistics.Contains(key))
            {
                return null;
            }

            var rawValue = connectionStatistics[key];
            if (rawValue == null)
            {
                return null;
            }

            if (rawValue is int intValue)
            {
                return intValue;
            }

            if (rawValue is long longValue)
            {
                return longValue;
            }

            return null;
        }
    }
}
