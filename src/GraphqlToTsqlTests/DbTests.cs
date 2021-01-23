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
            const string graphql = "{ orders (id: 1) { id } }";
            var graphqlParameters = new Dictionary<string, object> { };

            var expectedObject = new { orders = new[] { new { id = 1 } } };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task BoolValueQueryTest()
        {
            const string graphql = "{ badges (isSpecial: true) { name isSpecial } }";
            var graphqlParameters = new Dictionary<string, object> { { "isSpecial", true } };

            var expectedObject = new
            {
                badges = new[]
                {
                    new { name = "Diamond", isSpecial = true },
                    new { name = "Founder", isSpecial = true }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task DateQueryTest()
        {
            var graphql = @"
{
  orders (date: ""2020-01-29"") {
    id date seller { name sellerBadges(first: 1) { badgeName dateAwarded } }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "isActiv", true } };

            var expectedObject = new
            {
                orders = new[]
                {
                    new {
                        id = 2,
                        date = "2020-01-29",
                        seller = new {
                            name = "Bill",
                            sellerBadges = new[] {
                                new { badgeName = "Diamond", dateAwarded = "2020-04-07" }
                            }
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
