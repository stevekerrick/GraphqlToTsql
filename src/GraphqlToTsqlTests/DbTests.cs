using Dapper;
using DemoEntities;
using GraphqlToTsql;
using Newtonsoft.Json;
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
            var expectedObject = new { epcs = new[] { new { id = 1 } } };
            await CheckAsync(graphQl, null, expectedObject);
        }

        private static async Task CheckAsync(string graphQl, Dictionary<string, object> variableValues, object expectedObject)
        {
            var entityList = new DemoEntityList();
            var translator = new GraphqlTranslator(entityList);
            var result = translator.Translate(graphQl, variableValues);
            Assert.IsTrue(result.IsSuccessful, $"The parse failed: {result.ParseError}");

            // Query the database
            var tsql = result.Tsql;
            Console.WriteLine(tsql);
            var actualJson = await QueryAsync(tsql, result.TsqlParameters);

            // Compare
            var actualObj = JsonConvert.DeserializeObject(actualJson);
            var actualFormattedJson = JsonConvert.SerializeObject(actualObj, Formatting.Indented);
            Console.WriteLine(actualFormattedJson);
            var expectedFormattedJson = JsonConvert.SerializeObject(expectedObject, Formatting.Indented);
            Assert.AreEqual(expectedFormattedJson, actualFormattedJson, "Database response does not match expected");
        }

        private static async Task<string> QueryAsync(string tsql, Dictionary<string, object> tsqlParameters)
        {
            var parameters = new DynamicParameters(tsqlParameters);

            using (var connection = new SqlConnection(_connectionString))
            {
                var json = await connection.QuerySingleOrDefaultAsync<string>(tsql, parameters);
                return json;
            }
        }
    }
}
