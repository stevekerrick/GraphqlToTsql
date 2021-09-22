using DemoEntities;
using GraphqlToTsql;
using GraphqlToTsql.Entities;
using GraphqlToTsql.Introspection;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GraphqlToTsqlTests.TsqlTests
{
    public class TsqlTestBase : IntegrationTestBase
    {
        protected void Check(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            string expectedSql,
            Dictionary<string, object> expectedTsqlParameters,
            EmptySetBehavior emptySetBehavior = EmptySetBehavior.Null)
        {
            var result = Translate(graphql, graphqlParameters, emptySetBehavior);

            // Show the difference between Expected and Actual Tsql
            expectedSql = expectedSql.TrimEnd();
            var actualSql = result.Tsql.TrimEnd();
            var expectedLines = expectedSql.Split('\n');
            var actualLines = (actualSql ?? "").Split('\n');
            var errorShown = false;

            for (var i = 0; i < Math.Max(expectedLines.Length, actualLines.Length); i++)
            {
                var line1 = i < expectedLines.Length ? expectedLines[i].TrimEnd() : "";
                var line2 = i < actualLines.Length ? actualLines[i].TrimEnd() : "";
                Console.WriteLine(line2);
                if (!errorShown && line1 != line2)
                {
                    var diff = 0;
                    while (diff < line1.Length - 1 && diff < line2.Length - 1 && line1[diff] == line2[diff])
                    {
                        diff++;
                    }
                    var arrow = "".PadRight(diff, '~') + '^';
                    Console.WriteLine(arrow);
                    errorShown = true;
                }
            }
            if (errorShown)
            {
                Assert.Fail("Unexpected Sql result");
            }

            // Show the difference between Expected and Actual TsqlParameters
            var actualTsqlParameters = result.TsqlParameters;
            Console.WriteLine("");
            Console.WriteLine(JsonConvert.SerializeObject(actualTsqlParameters, Formatting.Indented));
            var errorCount = 0;
            foreach (var kv in expectedTsqlParameters)
            {
                if (!actualTsqlParameters.ContainsKey(kv.Key))
                {
                    Console.WriteLine($"Expected Tsql Parameter is missing: {kv.Key} = {kv.Value}");
                    errorCount++;
                }
                else if (kv.Value?.ToString() != actualTsqlParameters[kv.Key]?.ToString())
                {
                    Console.WriteLine($"Tsql Parameter has incorrect value, parameter [{kv.Key}], expected [{kv.Value}], actual [{actualTsqlParameters[kv.Key]}]");
                    errorCount++;
                }
            }
            foreach (var kv in actualTsqlParameters)
            {
                if (!expectedTsqlParameters.ContainsKey(kv.Key))
                {
                    Console.WriteLine($"Unexpected Tsql Parameter: {kv.Key} = {kv.Value}");
                    errorCount++;
                }
            }
            Assert.IsTrue(errorCount == 0, $"{errorCount} Tsql Parameter errors");
        }

        protected TsqlResult Translate(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            EmptySetBehavior emptySetBehavior = EmptySetBehavior.Null)
        {
            var allEntities = new List<EntityBase>();
            allEntities.AddRange(DemoEntityList.All());
            allEntities.AddRange(IntrospectionEntityList.All());

            var settings = new GraphqlActionSettings
            {
                AllowIntrospection = true,
                EmptySetBehavior = emptySetBehavior,
                EntityList = DemoEntityList.All()
            };

            var actions = new GraphqlActions();
            var tsqlResult = actions.TranslateToTsql(graphql, graphqlParameters, settings);

            Assert.IsNull(tsqlResult.Error, $"TSQL generation failed: {tsqlResult.Error}");

            return tsqlResult;
        }
    }
}
