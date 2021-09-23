using GraphqlToTsql.Translator;
using GraphqlToTsql.Util;
using NUnit.Framework;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsqlTests.TsqlTests
{
    public class PagingTsqlTests : TsqlTestBase
    {
        [Test]
        public void TotalCountTest()
        {
            var graphql = @"
{ sellers { name ordersConnection { totalCount } } }
".Trim();

            var expectedSql = @"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]

      -- sellers.ordersConnection (t2)
    , JSON_QUERY ((
        SELECT
          (SELECT COUNT(1) FROM [Order] t3 WHERE t1.[Name] = t3.[SellerName]) AS [totalCount]
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [ordersConnection]
    FROM [Seller] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void FirstOffsetTest()
        {
            const string graphql = "{ sellers (offset: 10, first: 2) { city } }";

            var expectedSql = @"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[City] AS [city]
    FROM [Seller] t1
    ORDER BY t1.[Name]
    OFFSET 10 ROWS
    FETCH FIRST 2 ROWS ONLY
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void FirstOffsetCompositeKeyTest()
        {
            const string graphql = "{ sellerBadges (offset: 10, first: 2) { dateAwarded } }";

            var expectedSql = @"
SELECT

  -- sellerBadges (t1)
  JSON_QUERY ((
    SELECT
      t1.[DateAwarded] AS [dateAwarded]
    FROM [SellerBadge] t1
    ORDER BY t1.[SellerName], t1.[BadgeName]
    OFFSET 10 ROWS
    FETCH FIRST 2 ROWS ONLY
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellerBadges]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void EdgeNodeTest()
        {
            var graphql = @"
{
  sellers (name: ""bill"") {
    city ordersConnection {
      edges {
        node { date shipping }
      }
    }
  }
}
".Trim();

            var expectedSql = @"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[City] AS [city]

      -- sellers.ordersConnection (t2)
    , JSON_QUERY ((
        SELECT

          -- sellers.ordersConnection.edges (t2)
          JSON_QUERY ((
            SELECT

              -- sellers.ordersConnection.edges.node (t2)
              JSON_QUERY ((
                SELECT
                  t2.[Date] AS [date]
                , t2.[Shipping] AS [shipping]
                FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [node]
            FROM [Order] t2
            WHERE t1.[Name] = t2.[SellerName]
            FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [edges]
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [ordersConnection]
    FROM [Seller] t1
    WHERE t1.[Name] = @name
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "name", "bill" } };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void OutputCursorTest()
        {
            var graphql = @"
{
  sellers {
    ordersConnection {
      edges {
        node { id }
        cursor
      }
    }
  }
}
".Trim();

            var expectedSql = @$"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT

      -- sellers.ordersConnection (t2)
      JSON_QUERY ((
        SELECT

          -- sellers.ordersConnection.edges (t2)
          JSON_QUERY ((
            SELECT

              -- sellers.ordersConnection.edges.node (t2)
              JSON_QUERY ((
                SELECT
                  t2.[Id] AS [id]
                FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [node]
            , ({CursorUtility.TsqlCursorDataFunc(ValueType.Int, "t2", "Order", "Id")}) AS [cursor]
            FROM [Order] t2
            WHERE t1.[Name] = t2.[SellerName]
            FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [edges]
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [ordersConnection]
    FROM [Seller] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void FirstAfterTest()
        {
            var after = 999;
            var cursor = CursorUtility.CreateCursor(new Value(after), "Order");

            var graphql = @"
query FirstAfterTest($cursor: String) {
  sellers {
    ordersConnection (first: 3, after: $cursor) {
      edges {
        node { shipping }
      }
    }
  }
}
".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "cursor", cursor } };

            var expectedSql = @"
-------------------------------
-- Operation: FirstAfterTest
-------------------------------

SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT

      -- sellers.ordersConnection (t2)
      JSON_QUERY ((
        SELECT

          -- sellers.ordersConnection.edges (t2)
          JSON_QUERY ((
            SELECT

              -- sellers.ordersConnection.edges.node (t2)
              JSON_QUERY ((
                SELECT
                  t2.[Shipping] AS [shipping]
                FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [node]
            FROM [Order] t2
            WHERE t1.[Name] = t2.[SellerName] AND t2.[Id] > @id
            ORDER BY t2.[Id]
            OFFSET 0 ROWS
            FETCH FIRST 3 ROWS ONLY
            FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [edges]
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [ordersConnection]
    FROM [Seller] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();

            var expectedTsqlParameters = new Dictionary<string, object> { { "id", after } };

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void FirstOffsetWithFilterTest()
        {
            const string graphql = "{ sellers (offset: 10, first: 2, city: \"Kona\") { name } }";

            var expectedSql = @"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Seller] t1
    WHERE t1.[City] = @city
    ORDER BY t1.[Name]
    OFFSET 10 ROWS
    FETCH FIRST 2 ROWS ONLY
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "city", "Kona" } };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void FirstAfterWithFilterTest()
        {
            var after = 999;
            var cursor = CursorUtility.CreateCursor(new Value(after), "Order");

            var graphql = @"
query FirstAfterWithFilterTest($cursor: String) {
  sellers {
    ordersConnection (first: 3, after: $cursor, shipping: 9.95) {
      edges {
        node { date }
      }
    }
  }
}
".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "cursor", cursor } };

            var expectedSql = @"
-------------------------------
-- Operation: FirstAfterWithFilterTest
-------------------------------

SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT

      -- sellers.ordersConnection (t2)
      JSON_QUERY ((
        SELECT

          -- sellers.ordersConnection.edges (t2)
          JSON_QUERY ((
            SELECT

              -- sellers.ordersConnection.edges.node (t2)
              JSON_QUERY ((
                SELECT
                  t2.[Date] AS [date]
                FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [node]
            FROM [Order] t2
            WHERE t1.[Name] = t2.[SellerName] AND t2.[Shipping] = @shipping AND t2.[Id] > @id
            ORDER BY t2.[Id]
            OFFSET 0 ROWS
            FETCH FIRST 3 ROWS ONLY
            FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [edges]
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [ordersConnection]
    FROM [Seller] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();

            var expectedTsqlParameters = new Dictionary<string, object> { { "id", after }, { "shipping", 9.95 } };

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void PagingOnMainQueryTest()
        {
            var graphql = @"
{
  ordersConnection (first: 10, offset: 100) {
    edges {
      node { date }
    }
  }
}
".Trim();
            var graphqlParameters = new Dictionary<string, object>();

            var expectedSql = @"
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
              t1.[Date] AS [date]
            FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [node]
        FROM [Order] t1
        ORDER BY t1.[Id]
        OFFSET 100 ROWS
        FETCH FIRST 10 ROWS ONLY
        FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [edges]
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [ordersConnection]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();

            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }
    }
}
