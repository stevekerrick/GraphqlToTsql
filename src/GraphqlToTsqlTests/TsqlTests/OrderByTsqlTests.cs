using GraphqlToTsql.Translator;
using GraphqlToTsql.Util;
using NUnit.Framework;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsqlTests.TsqlTests
{
    public class OrderByTsqlTests : TsqlTestBase
    {
        [Test]
        public void OrderBy_NonKeyFieldTest([Values] bool isAscending)
        {
            var graphqlDirection = isAscending ? "ASC" : "DESC";
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
        public void OrderBy_KeyFieldTest()
        {
            var graphql = "{ sellers (order_by: { name: DESC }) { name } }";

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
        public void OrderBy_CursorBasedPagingTest()
        {
            var cursor = CursorUtility.CreateCursor(new Value(ValueType.Int, 99), "Order");

            var graphql = "{ ordersConnection (first: 10, after: \"" + cursor + "\", order_by: { id: DESC}) { edges { node { id } } } }";

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
        public void OrderBy_OffsetPagingTest([Values] bool isAscending)
        {
            var graphqlDirection = isAscending ? "ASC" : "DESC";
            var graphql = "{ sellers (first: 5, offset: 10, order_by: { city: " + graphqlDirection + "}) { name } }";

            var sqlDirection = isAscending ? "" : " DESC";
            var expectedSql = @$"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Seller] t1
    ORDER BY t1.[City]{sqlDirection}, t1.[Name]{sqlDirection}
    OFFSET 10 ROWS
    FETCH FIRST 5 ROWS ONLY
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void OrderBy_SingleColumn_LiteralValue()
        {
            var graphql = @"query OrderByCity { sellers (order_by: {city: DESC}) { name } }";
            var graphqlParameters = new Dictionary<string, object> { { "orderBy", new { city = "DESC" } } };
            CheckOrderByCity(graphql, graphqlParameters);
        }

        [Test]
        public void OrderBy_SingleColumn_Variable()
        {
            var graphql = @"query OrderByCity ($orderBy: OrderBy) { sellers (order_by: $orderBy) { name } }";
            var graphqlParameters = new Dictionary<string, object> { { "orderBy", new { city = "DESC" } } };
            CheckOrderByCity(graphql, graphqlParameters);
        }

        [Test]
        public void OrderBy_SingleColumn_VariableWithDefaultValue()
        {
            var graphql = @"query OrderByCity ($orderBy: OrderBy={city: DESC}) { sellers (order_by: $orderBy) { name } }";
            var graphqlParameters = new Dictionary<string, object> { };
            CheckOrderByCity(graphql, graphqlParameters);
        }

        [Test]
        public void OrderBy_SingleColumn_VariableIgnoreDefaultValue()
        {
            var graphql = @"query OrderByCity ($orderBy: OrderBy={postalCode: DESC}) { sellers (order_by: $orderBy) { name } }";
            var graphqlParameters = new Dictionary<string, object> { { "orderBy", new { city = "DESC" } } };
            CheckOrderByCity(graphql, graphqlParameters);
        }

        private void CheckOrderByCity(string graphql, Dictionary<string, object> graphqlParameters)
        {
            var expectedSql = @"
-------------------------------
-- Operation: OrderByCity
-------------------------------

SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Seller] t1
    ORDER BY t1.[City] DESC, t1.[Name] DESC
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { };

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void OrderBy_MultiColumn_LiteralValue()
        {
            var graphql = @"query OrderByCityAndState { sellers (order_by: [{city: DESC}, {state: ASC}]) { name } }";
            var graphqlParameters = new Dictionary<string, object> { { "orderBy", new { city = "DESC" } } };
            CheckOrderByCityAndState(graphql, graphqlParameters);
        }

        [Test]
        public void OrderBy_MultiColumn_Variable()
        {
            var graphql = @"query OrderByCityAndState ($orderBy: OrderBy) { sellers (order_by: $orderBy) { name } }";
            var graphqlParameters = new Dictionary<string, object> { { "orderBy", new object[] { new { city = "DESC" }, new { state = "ASC" } } } };
            CheckOrderByCityAndState(graphql, graphqlParameters);
        }

        [Test]
        public void OrderBy_MultiColumn_VariableWithDefaultValue()
        {
            var graphql = @"query OrderByCityAndState ($orderBy: OrderBy=[{city: DESC}, {state: ASC}]) { sellers (order_by: $orderBy) { name } }";
            var graphqlParameters = new Dictionary<string, object> { };
            CheckOrderByCityAndState(graphql, graphqlParameters);
        }

        [Test]
        public void OrderBy_MultiColumn_VariableIgnoreDefaultValue()
        {
            var graphql = @"query OrderByCityAndState ($orderBy: OrderBy={postalCode: DESC}) { sellers (order_by: $orderBy) { name } }";
            var graphqlParameters = new Dictionary<string, object> { { "orderBy", new object[] { new { city = "DESC" }, new { state = "ASC" } } } };
            CheckOrderByCityAndState(graphql, graphqlParameters);
        }

        private void CheckOrderByCityAndState(string graphql, Dictionary<string, object> graphqlParameters)
        {
            var expectedSql = @"
-------------------------------
-- Operation: OrderByCityAndState
-------------------------------

SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Seller] t1
    ORDER BY t1.[City] DESC, t1.[State], t1.[Name] DESC
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { };

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void OrderBy_CalculatedFieldTest()
        {
            var graphql = "{ products (order_by: { totalRevenue: DESC }) { name totalRevenue } }";

            var expectedSql = @"
SELECT

  -- products (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    , (SELECT (SELECT SUM(od.Quantity) FROM OrderDetail od WHERE t1.[Name] = od.ProductName) * t1.Price) AS [totalRevenue]
    FROM [Product] t1
    ORDER BY (SELECT (SELECT SUM(od.Quantity) FROM OrderDetail od WHERE t1.[Name] = od.ProductName) * t1.Price) DESC, t1.[Name] DESC
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [products]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void OrderBy_NonObjectValue_Fails()
        {
            var graphql = "{ sellers (order_by: \"city\") { name } }";
            ParseShouldFail(graphql, null, "Invalid order_by value. Try something like { id: DESC }.");
        }

        [Test]
        public void OrderBy_MultipleColumnsInSingleObject_Fails()
        {
            var graphql = "{ sellers (order_by: { city: ASC, state: ASC }) { name } }";
            ParseShouldFail(graphql, null, "Invalid order_by value. Try something like { id: DESC }.");
        }

        [Test]
        public void OrderBy_EmptyObject_Fails()
        {
            var graphql = "{ sellers (order_by: { }) { name } }";
            ParseShouldFail(graphql, null, "Invalid order_by value. Try something like { id: DESC }.");
        }

        [Test]
        public void OrderBy_InvalidDirection_String_Fails()
        {
            var graphql = "{ sellers (order_by: { city: \"foo\" }) { name } }";
            ParseShouldFail(graphql, null, "Invalid order_by value. Try something like { id: DESC }.");
        }

        [Test]
        public void OrderBy_InvalidDirection_Integer_Fails()
        {
            var graphql = "{ sellers (order_by: { city: 1234 }) { name } }";
            ParseShouldFail(graphql, null, "Invalid order_by value. Try something like { id: DESC }.");
        }

        [Test]
        public void OrderBy_InvalidDirection_NotAscOrDesc_Fails()
        {
            var graphql = "{ sellers (order_by: { city: up }) { name } }";
            ParseShouldFail(graphql, null, "Invalid order_by value. Try something like { id: DESC }.");
        }

        [Test]
        public void OrderBy_Cursor_Conflict_Fails()
        {
            var cursor = CursorUtility.CreateCursor(new Value(ValueType.Int, 99), "Order");
            var graphql = "{ ordersConnection (first: 10, after: \"" + cursor + "\", order_by: { date: DESC}) { edges { node { id } } } }";
            ParseShouldFail(graphql, null, "Because you are using cursor-based paging, you can only order_by id");
        }

        [Test]
        public void OrderBy_Cursor_Conflict2_Fails()
        {
            var cursor = CursorUtility.CreateCursor(new Value(ValueType.Int, 99), "Order");
            var graphql = "{ ordersConnection (order_by: { date: DESC}, first: 10, after: \"" + cursor + "\") { edges { node { id } } } }";
            ParseShouldFail(graphql, null, "Because you are using cursor-based paging, you can only order_by id");
        }

        [Test]
        public void OrderBy_NonScalarField_Fails()
        {
            var graphql = "{ sellers (order_by: { orders: DESC }) { name } }";
            ParseShouldFail(graphql, null, "order_by is not allowed on [sellers.orders]");
        }
    }
}
