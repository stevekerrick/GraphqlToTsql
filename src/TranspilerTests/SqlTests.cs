using System;
using NUnit.Framework;
using GraphqlToSql.Transpiler.Transpiler;

namespace GraphqlToSql.TranspilerTests
{
    [TestFixture]
    public class SqlTests
    {
        [Test]
        public void SimpleQueryTest()
        {
            const string graphQl = "{ codes { secureCode } }";
            var expectedSql = @"
SELECT

  -- codes
  JSON_QUERY ((
    SELECT
      t1.SecureCode AS secureCode
    FROM Code t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS codes

FOR JSON PATH, INCLUDE_NULL_VALUES
".Trim();
            Check(graphQl, expectedSql);
        }

//         [Test]
//         public void AliasTest()
//         {
//             const string graphQl = "{ myCodes: codes { id foo: secureCode } }";
//             var expectedSql = @"
// WITH cte1(cte1Json) AS (
//   SELECT
//     CodeID AS id
//   , SecureCode AS foo
//   FROM Code
//   FOR JSON AUTO, INCLUDE_NULL_VALUES
// )
// SELECT
//   cte1Json AS myCodes
// FROM cte1
// FOR JSON AUTO, INCLUDE_NULL_VALUES
// ".Trim();
//             Check(graphQl, expectedSql);
//         }

        private static void Check(string graphQl, string expectedSql)
        {
            var translator = new Translator();
            var result = translator.Translate(graphQl);
            Assert.IsTrue(result.IsSuccessful, $"The parse failed: {result.ParseError}");

            // Show the difference between Expected and Actual
            expectedSql = expectedSql.TrimEnd();
            var actualSql = result.Query.Command.TrimEnd();
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
