using DemoEntities;
using GraphqlToTsql;
using GraphqlToTsql.Translator;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class TsqlTests
    {
        [Test]
        public void SimpleQueryTest()
        {
            const string graphQl = "{ epcs { urn } }";

            var expectedSql = @"
SELECT

  -- epcs
  JSON_QUERY ((
    SELECT
      t1.[Urn] AS [urn]
    FROM [Epc] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [epcs]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphQl, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void AliasTest()
        {
            const string graphQl = "{ codes: epcs { MyUrl: urn } }";

            var expectedSql = @"
SELECT

  -- codes
  JSON_QUERY ((
    SELECT
      t1.[Urn] AS [MyUrl]
    FROM [Epc] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [codes]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphQl, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void ArgumentTest()
        {
            const string graphQl = "{ epcs (id: 1) { urn } }";

            var expectedSql = @"
SELECT

  -- epcs
  JSON_QUERY ((
    SELECT
      t1.[Urn] AS [urn]
    FROM [Epc] t1
    WHERE t1.[Id] = @id
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [epcs]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> {
                {"id", 1 }
            };

            Check(graphQl, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void JoinTest()
        {
            const string graphQl = "{ epcs { urn product { name } } }";

            var expectedSql = @"
SELECT

  -- epcs
  JSON_QUERY ((
    SELECT
      t1.[Urn] AS [urn]

      -- epcs.product
    , JSON_QUERY ((
        SELECT
          t2.[Name] AS [name]
        FROM [Product] t2
        WHERE t1.[ProductId] = t2.[Id]
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [product]
    FROM [Epc] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [epcs]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphQl, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void VariableTest()
        {
            const string graphQl = "query VariableTest($idVar: ID, $urnVar: String = \"bill\") { epcs (id: $idVar, urn: $urnVar) { urn } }";
            var variableValues = new Dictionary<string, object> { { "idVar", 2 } };

            var expectedSql = @"
-------------------------------
-- Operation: VariableTest
-------------------------------

SELECT

  -- epcs
  JSON_QUERY ((
    SELECT
      t1.[Urn] AS [urn]
    FROM [Epc] t1
    WHERE t1.[Id] = @idVar AND t1.[Urn] = @urnVar
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [epcs]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> {
                { "idVar", 2 },
                { "urnVar", "bill" }
            };

            Check(graphQl, variableValues, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void CalculatedFieldTest()
        {
            const string graphQl = "{ epcs { urn dispositionName } }";

            var expectedSql = @"
SELECT

  -- epcs
  JSON_QUERY ((
    SELECT
      t1.[Urn] AS [urn]
    , (SELECT d.Name FROM Disposition d WHERE d.Id = t1.DispositionId) AS [dispositionName]
    FROM [Epc] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [epcs]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphQl, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void FragmentTest()
        {
            //TODO: Implement enough of a type system so that the fragment can be defined for the type "Epc" (not "epc")
            var graphQl = @"
{ epcs { ... frag} }
fragment frag on epc { urn }
".Trim();

            var expectedSql = @"
SELECT

  -- epcs
  JSON_QUERY ((
    SELECT
      t1.[Urn] AS [urn]
    FROM [Epc] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [epcs]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphQl, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void ComplicatedQueryTest()
        {
            var graphQl = @"
query jojaCola ($urn: string) {
  product1: product (urn: $urn) {
    name urn
    lots {
      lotNumber expirationDate
      epcs {
        urn disposition { name }
        bizLocation { urn name }
        readPoint { urn name }
        lastUpdate
        children { urn disposition { name } }
      }
    }
  }
}
".Trim();
            var variableValues = new Dictionary<string, object> { { "urn", "urn:epc:idpat:sgtin:258643.3704146.*" } };

            var result = Translate(graphQl, variableValues);
            var tsql = result.Tsql;
            Assert.IsTrue(tsql.Contains("jojaCola"));
            Assert.IsTrue(tsql.Contains("Product"));
            Assert.IsTrue(tsql.Contains("Lot"));
            Assert.IsTrue(tsql.Contains("Epc"));
            Assert.IsTrue(tsql.Contains("Disposition"));
            Assert.IsTrue(tsql.Contains("Location"));
        }

        [Test]
        public void TotalCountTest()
        {
            var graphQl = @"
{ products { urn lotsConnection { totalCount } } }
".Trim();

            var expectedSql = @"
SELECT

  -- products
  JSON_QUERY ((
    SELECT
      t1.[Urn] AS [urn]

      -- products.lotsConnection
    , JSON_QUERY ((
        SELECT
          (SELECT COUNT(1) FROM [Lot] t2 WHERE t1.[Id] = t2.[ProductId]) AS [totalCount]
        FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [lotsConnection]
    FROM [Product] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [products]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphQl, null, expectedSql, expectedTsqlParameters);
        }



        private static TranslateResult Translate(string graphQl, Dictionary<string, object> variableValues)
        {
            var entityList = new DemoEntityList();
            var translator = new GraphqlTranslator(entityList);
            var result = translator.Translate(graphQl, variableValues);
            Assert.IsTrue(result.IsSuccessful, $"The parse failed: {result.ParseError}");

            return result;
        }

        private static void Check(
            string graphQl,
            Dictionary<string, object> variables,
            string expectedSql,
            Dictionary<string, object> expectedTsqlParameters)
        {
            var result = Translate(graphQl, variables);

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
    }
}
