using NUnit.Framework;
using System.Collections.Generic;

namespace GraphqlToTsqlTests.TsqlTests
{
    public class OrderByTsqlTests : TsqlTestBase
    {
        [Test]
        public void OrderByNonKeyFieldTest([Values] bool isAscending)
        {
            var asc = isAscending ? "asc" : "desc";
            var graphql = "{ sellers (order_by: { city: " + asc + "}) { name } }";

            var expectedSql = @$"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Seller] t1
    ORDER BY [City] {asc.ToUpper()}, [Name] ASC
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }
    }
}
