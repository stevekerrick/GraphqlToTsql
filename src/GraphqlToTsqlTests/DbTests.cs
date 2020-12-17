using Dapper;
using DemoEntities;
using GraphqlToTsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class DbTests
    {
        private const string _connectionString = "Data Source=MIN-DT180101;Initial Catalog=GraphqlToTsqlTests;Integrated Security=True";

        [Test]
        public async Task SimpleQueryTest()
        {
            const string graphQl = "{ epcs (id: 1) { id } }";
            var expectedResult = new { epcs = new[] { new { id = 1 } } };
            await CheckAsync(graphQl, null, expectedResult);
        }

        private static async Task CheckAsync(string graphQl, Dictionary<string, object> variableValues, object expectedJson)
        {
            var entityList = new DemoEntityList();
            var translator = new GraphqlTranslator(entityList);
            var result = translator.Translate(graphQl, variableValues);
            Assert.IsTrue(result.IsSuccessful, $"The parse failed: {result.ParseError}");

            // Query the database
            var sql = result.Tsql;
            Console.WriteLine(sql);
            var json = await QueryAsync(sql);
            Console.WriteLine(json);
        }

        private static async Task<string> QueryAsync(string sql)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var json = await connection.QuerySingleOrDefaultAsync<string>(sql);
                return json;
            }
        }
    }
}
