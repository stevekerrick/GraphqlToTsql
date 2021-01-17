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
        [Test]
        public async Task SimpleQueryTest()
        {
            const string graphql = "{ epcs (id: 1) { id } }";
            var graphqlParameters = new Dictionary<string, object> { };

            var expectedObject = new { epcs = new[] { new { id = 1 } } };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task BoolValueQueryTest()
        {
            const string graphql = "{ locations (isActive: false) { name isActive } }";
            var graphqlParameters = new Dictionary<string, object> { { "isActiv", true } };

            var expectedObject = new
            {
                locations = new[]
                {
                    new { name = "Silver Foods Warehouse", isActive = false },
                    new { name = "Silver Foods #1", isActive = false },
                    new { name = "Silver Foods #2", isActive = false }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task DateQueryTest()
        {
            var graphql = @"
{
  lots (expirationDate: ""2020-01-31"") {
    lotNumber expirationDate epcs (first: 1) { id lastUpdate }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "isActiv", true } };

            var expectedObject = new
            {
                lots = new[]
                {
                    new {
                        lotNumber = "LOT 2001a",
                        expirationDate = "2020-01-31",
                        epcs = new[]
                        {
                            new { id = 8, lastUpdate = "2019-04-01T16:00:00Z" }
                        }
                    }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }


        private async Task CheckAsync(string graphql, Dictionary<string, object> graphqlParameters, object expectedObject)
        {
            var runner = GetService<IRunner>();
            var runnerResult = await runner.TranslateAndRun(graphql, graphqlParameters, DemoEntityList.All());

            Assert.IsNull(runnerResult.ParseError, $"The parse failed: {runnerResult.ParseError}");
            Console.WriteLine(runnerResult.Tsql);
            Console.WriteLine(JsonConvert.SerializeObject(runnerResult.TsqlParameters, Formatting.Indented));

            Assert.IsNull(runnerResult.DbError, $"The database query failed: {runnerResult.DbError}");
            var dataObj = JsonConvert.DeserializeObject(runnerResult.DataJson);
            var dataFormattedJson = JsonConvert.SerializeObject(dataObj, Formatting.Indented);
            Console.WriteLine("");
            Console.WriteLine(dataFormattedJson);

            var expectedFormattedJson = JsonConvert.SerializeObject(expectedObject, Formatting.Indented);
            Assert.AreEqual(expectedFormattedJson, dataFormattedJson, "Database response does not match expected");
        }
    }
}
