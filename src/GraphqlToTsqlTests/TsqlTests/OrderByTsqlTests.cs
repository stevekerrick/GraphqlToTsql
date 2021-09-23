using NUnit.Framework;
using System.Collections.Generic;

namespace GraphqlToTsqlTests.TsqlTests
{
    public class OrderByTsqlTests : TsqlTestBase
    {
        [Test]
        public void OrderByNonKeyFieldTest([Values] bool isAscending)
        {
            var graphqlDirection = isAscending ? "asc" : "desc";
            var graphql = "{ sellers (order_by: { city: " + graphqlDirection + "}) { name } }";

            var sqlDirection = isAscending ? "" : " DESC";
            var expectedSql = @$"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Seller] t1
    ORDER BY t1.[City]{sqlDirection}, t1.[Name]{sqlDirection}
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void OrderBy_NonObjectValue_Fails()
        {
            var graphql = "{ sellers (order_by: \"city\") { name } }";
            ParseShouldFail(graphql, null, "An object value was expected");
        }

        [Test]
        public void OrderBy_MultipleColumnsInSingleObject_Fails()
        {
            var graphql = "{ sellers (order_by: { city: asc, state: asc }) { name } }";
            ParseShouldFail(graphql, null, "order_by must specify exactly one field to order by");
        }

        [Test]
        public void OrderBy_EmptyObject_Fails()
        {
            var graphql = "{ sellers (order_by: { }) { name } }";
            ParseShouldFail(graphql, null, "order_by must specify exactly one field to order by");
        }

        [Test]
        public void OrderBy_InvalidDirection_Fails()
        {
            var graphql = "{ sellers (order_by: { city: \"foo\" }) { name } }";
            ParseShouldFail(graphql, null, "Expected an unquoted enum value");
        }

        [Test]
        public void OrderBy_InvalidDirection_Fails2()
        {
            var graphql = "{ sellers (order_by: { city: 1234 }) { name } }";
            ParseShouldFail(graphql, null, "Expected an unquoted enum value");
        }

        [Test]
        public void OrderBy_InvalidDirection_Fails3()
        {
            var graphql = "{ sellers (order_by: { city: up }) { name } }";
            ParseShouldFail(graphql, null, "Expected one of (asc, desc)");
        }


        // use enum for asc/desc instead of string
        // use variable for asc/desc value
        // use variable for column name
        // TODO: refactor so that objectValue parsing is more general-purpose, and uses enums properly
        // order_by combined with paging
        // order_by on a connection
        // TODO: support multiple order_by's
        // TODO: Introspection support
        // TODO: Documentation
        // TODO: Sample queries
        // Don't allow cursor-based paging when there's non-PK OrderBy



    }


}
