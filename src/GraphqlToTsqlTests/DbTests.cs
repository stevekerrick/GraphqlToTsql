using DemoEntities;
using GraphqlToTsql;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class DbTests : IntegrationTestBase
    {
        private const string _connectionString = "Data Source=MIN-DT180101;Initial Catalog=GraphqlToTsqlTests;Integrated Security=True";

        [Test]
        public async Task SimpleQueryTest()
        {
            const string graphQl = "{ epcs (id: 1) { id } }";
            var expectedObject = new { epcs = new[] { new { id = 1 } } };
            await CheckAsync(graphQl, null, expectedObject);
        }

        private async Task CheckAsync(string graphQl, Dictionary<string, object> graphqlParameters, object expectedObject)
        {
            var translator = GetService<IGraphqlTranslator>();
            var result = await translator.Translate(graphQl, graphqlParameters, DemoEntityList.All());

            Assert.IsNull(result.ParseError, $"The parse failed: {result.ParseError}");
            Console.WriteLine(result.Tsql);
            Console.WriteLine(JsonConvert.SerializeObject(result.TsqlParameters, Formatting.Indented));

            Assert.IsNull(result.DbError, $"The database query failed: {result.DbError}");
            var dataObj = JsonConvert.DeserializeObject(result.DataJson);
            var dataFormattedJson = JsonConvert.SerializeObject(dataObj, Formatting.Indented);
            Console.WriteLine("");
            Console.WriteLine(dataFormattedJson);

            var expectedFormattedJson = JsonConvert.SerializeObject(expectedObject, Formatting.Indented);
            Assert.AreEqual(expectedFormattedJson, dataFormattedJson, "Database response does not match expected");
        }
    }
}
