using GraphqlToTsql.Translator;
using NUnit.Framework;
using System;

namespace GraphqlToTsql.TranslatorTests
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

FOR JSON PATH, INCLUDE_NULL_VALUES
".Trim();
            Check(graphQl, null, expectedSql);
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

FOR JSON PATH, INCLUDE_NULL_VALUES
".Trim();
            Check(graphQl, null, expectedSql);
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
    WHERE t1.[Id] = 1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [epcs]

FOR JSON PATH, INCLUDE_NULL_VALUES
".Trim();
            Check(graphQl, null, expectedSql);
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

FOR JSON PATH, INCLUDE_NULL_VALUES
".Trim();
            Check(graphQl, null, expectedSql);
        }

        [Test]
        public void VariableTest()
        {
            const string graphQl = "query VariableTest($idVar: ID, $urnVar: String = \"bill\") { epcs (id: $idVar, urn: $urnVar) { urn } }";
            var variables = new { idVar = 2 };
            var expectedSql = @"
SELECT

  -- epcs
  JSON_QUERY ((
    SELECT
      t1.[Urn] AS [urn]
    FROM [Epc] t1
    WHERE t1.[Id] = 2 AND t1.[Urn] = 'bill'
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [epcs]

FOR JSON PATH, INCLUDE_NULL_VALUES
".Trim();
            Check(graphQl, variables, expectedSql);
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
    , (SELECT d.DispositionName FROM Disposition d WHERE d.Id = t1.DispositionId) AS [dispositionName]
    FROM [Epc] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [epcs]

FOR JSON PATH, INCLUDE_NULL_VALUES
".Trim();
            Check(graphQl, null, expectedSql);
        }

        private static void Check(string graphQl, object variables, string expectedSql)
        {
            var translator = new GraphqlTranslator();
            var result = translator.Translate(graphQl, variables);
            Assert.IsTrue(result.IsSuccessful, $"The parse failed: {result.ParseError}");

            // Show the difference between Expected and Actual
            expectedSql = expectedSql.TrimEnd();
            var actualSql = result.Query.TrimEnd();
            if (expectedSql != actualSql)
            {
                var expectedLines = expectedSql.Split('\n');
                var actualLines = (actualSql ?? "").Split('\n');
                var errorShown = false;

                Console.WriteLine("------------------------------------");
                for (var i = 0; i < Math.Max(expectedLines.Length, actualLines.Length); i++)
                {
                    var line1 = i < expectedLines.Length ? expectedLines[i].TrimEnd() : "";
                    var line2 = i < actualLines.Length ? actualLines[i].TrimEnd() : "";
                    Console.WriteLine(line1.PadRight(60) + " : " + line2);
                    if (!errorShown && line1 != line2)
                    {
                        var diff = 0;
                        while (diff < line1.Length - 1 && diff < line2.Length - 1 && line1[diff] == line2[diff])
                        {
                            diff++;
                        }
                        var arrow = "".PadRight(diff, '=') + '^';
                        Console.WriteLine(arrow.PadRight(60) + " : " + arrow);
                        errorShown = true;
                    }
                }

                Assert.Fail("Unexpected Sql result");
            }
        }
    }
}
