using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DemoEntities
{
    public static class DbAccess
    {
        public static async Task<string> QueryAsync(
            string connectionString,
            string tsql,
            Dictionary<string, object> tsqlParameters)
        {
            var dapperTsqlParameters = ConvertTsqlParameters(tsqlParameters);
            var parameters = new DynamicParameters(dapperTsqlParameters);

            using (var connection = new SqlConnection(connectionString))
            {
                var json = await connection.QuerySingleOrDefaultAsync<string>(tsql, parameters);
                return json;
            }
        }

        // Dapper wants the parameter names prepended with @
        private static Dictionary<string, object> ConvertTsqlParameters(Dictionary<string, object> tsqlParameters)
        {
            if (tsqlParameters == null)
            {
                tsqlParameters = new Dictionary<string, object>();
            }

            var dapperTsqlParameters = new Dictionary<string, object>();
            foreach (var kv in tsqlParameters)
            {
                dapperTsqlParameters[$"@{kv.Key}"] = kv.Value;
            }

            return dapperTsqlParameters;
        }
    }
}
