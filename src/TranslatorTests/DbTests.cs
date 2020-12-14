using Dapper;
using GraphqlToTsql.Translator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace GraphqlToTsql.TranslatorTests
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
            var translator = new GraphqlTranslator();
            var result = translator.Translate(graphQl, variableValues);
            Assert.IsTrue(result.IsSuccessful, $"The parse failed: {result.ParseError}");

            // Query the database
            var sql = result.Query;
            Console.WriteLine(sql);
            var json = await QueryAsync(sql);
            Console.WriteLine(json);


            // Show the difference between Expected and Actual
            //expectedSql = expectedSql.TrimEnd();
            //var actualSql = result.Query.TrimEnd();
            //if (expectedSql != actualSql)
            //{
            //    var expectedLines = expectedSql.Split('\n');
            //    var actualLines = (actualSql ?? "").Split('\n');
            //    var errorShown = false;

            //    Console.WriteLine("------------------------------------");
            //    for (var i = 0; i < Math.Max(expectedLines.Length, actualLines.Length); i++)
            //    {
            //        var line1 = i < expectedLines.Length ? expectedLines[i].TrimEnd() : "";
            //        var line2 = i < actualLines.Length ? actualLines[i].TrimEnd() : "";
            //        Console.WriteLine(line1.PadRight(60) + " : " + line2);
            //        if (!errorShown && line1 != line2)
            //        {
            //            var diff = 0;
            //            while (diff < line1.Length - 1 && diff < line2.Length - 1 && line1[diff] == line2[diff])
            //            {
            //                diff++;
            //            }
            //            var arrow = "".PadRight(diff, '=') + '^';
            //            Console.WriteLine(arrow.PadRight(60) + " : " + arrow);
            //            errorShown = true;
            //        }
            //    }

            //    Assert.Fail("Unexpected Sql result");
            //}
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
