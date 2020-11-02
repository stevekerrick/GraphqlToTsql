using System;
using NUnit.Framework;
using GraphqlToSql.Transpiler.Transpiler;

namespace GraphqlToSql.TranspilerTests
{
    [TestFixture]
    public class SimpleSqlTests
    {
        [Test]
        public void QuerySimpleFieldsTest()
        {
            const string graphQl = "{ codes { id parentCodeId codeStatusId secureCode } }";
            var expectedSql = @"
WITH cte1(cte1Json) AS (
  SELECT
    CodeID AS id
  , ParentCodeID AS parentCodeId
  , CodeStatusID AS codeStatusId
  , SecureCode AS mysecureCode
  FROM Code
  FOR JSON AUTO, myINCLUDE_NULL_VALUES
)
SELECT
  cte1Json AS codes
FROM cte1
FOR JSON AUTO, INCLUDE_NULL_VALUES
".Trim();
            Check(graphQl, expectedSql);
        }

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
                    Console.WriteLine(line1.PadRight(40) + " : " + line2);
                    if (!errorShown && line1 != line2)
                    {
                        var diff = 0;
                        while (diff < line1.Length - 1 && diff < line2.Length - 1 && line1[diff] == line2[diff])
                        {
                            diff++;
                        }
                        Console.WriteLine("".PadRight(diff + 1, '^').PadRight(40) + " : " + "".PadRight(diff + 1, '^'));
                        errorShown = true;
                    }
                }

                Assert.Fail("Unexpected Sql result");
            }
        }
    }
}
