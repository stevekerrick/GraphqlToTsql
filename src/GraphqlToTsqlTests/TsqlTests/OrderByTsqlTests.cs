using GraphqlToTsql.Translator;
using GraphqlToTsql.Util;
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
        public void OrderByKeyFieldTest()
        {
            var graphql = "{ sellers (order_by: { name: desc }) { name } }";

            var expectedSql = @"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Seller] t1
    ORDER BY t1.[Name] DESC
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void CursorBasedPagingWithOrderByTest()
        {
            var cursor = CursorUtility.CreateCursor(new Value(99), "Order");

            var graphql = "{ ordersConnection (first: 10, after: \"" + cursor + "\", order_by: { id: desc}) { edges { node { id } } } }";

            var expectedSql = @$"
SELECT

  -- ordersConnection (t1)
  JSON_QUERY ((
    SELECT

      -- ordersConnection.edges (t1)
      JSON_QUERY ((
        SELECT

          -- ordersConnection.edges.node (t1)
          JSON_QUERY ((
            SELECT
              t1.[Id] AS [id]
            FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [node]
        FROM [Order] t1
        WHERE t1.[Id] < @id
        ORDER BY t1.[Id] DESC
        OFFSET 0 ROWS
        FETCH FIRST 10 ROWS ONLY
        FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [edges]
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [ordersConnection]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "id", 99 } };

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
        public void OrderBy_InvalidDirection_String_Fails()
        {
            var graphql = "{ sellers (order_by: { city: \"foo\" }) { name } }";
            ParseShouldFail(graphql, null, "Expected an unquoted enum value");
        }

        [Test]
        public void OrderBy_InvalidDirection_Integer_Fails()
        {
            var graphql = "{ sellers (order_by: { city: 1234 }) { name } }";
            ParseShouldFail(graphql, null, "Expected an unquoted enum value");
        }

        [Test]
        public void OrderBy_InvalidDirection_NotAscOrDesc_Fails()
        {
            var graphql = "{ sellers (order_by: { city: up }) { name } }";
            ParseShouldFail(graphql, null, "Expected one of (asc, desc)");
        }

        [Test]
        public void OrderBy_Cursor_Conflict_Fails()
        {
            var cursor = CursorUtility.CreateCursor(new Value(99), "Order");
            var graphql = "{ ordersConnection (first: 10, after: \"" + cursor + "\", order_by: { date: desc}) { edges { node { id } } } }";
            ParseShouldFail(graphql, null, "Because you are using cursor-based paging, you can only order_by id");
        }

        [Test]
        public void OrderBy_Cursor_Conflict2_Fails()
        {
            var cursor = CursorUtility.CreateCursor(new Value(99), "Order");
            var graphql = "{ ordersConnection (order_by: { date: desc}, first: 10, after: \"" + cursor + "\") { edges { node { id } } } }";
            ParseShouldFail(graphql, null, "Because you are using cursor-based paging, you can only order_by id");
        }




        // use variable for asc/desc value
        // use variable for entire OrderBy object
        // TODO: refactor so that objectValue parsing is more general-purpose, and uses enums properly
        // order_by combined with paging
        // TODO: support multiple order_by's
        // TODO: Introspection support
        // TODO: Documentation
        // TODO: Sample queries



    }


}
