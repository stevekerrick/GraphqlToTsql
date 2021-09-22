using NUnit.Framework;
using System.Collections.Generic;

namespace GraphqlToTsqlTests.TsqlTests
{
    public class DirectiveTsqlTests : TsqlTestBase
    {
        [Test]
        public void DirectiveInclude_TrueTest()
        {
            var graphql = @"
query MyOrder($id: Int!, $showSeller: Boolean!) {
  order (id: $id) {
    id
    seller @include(if: $showSeller) { name }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "id", 10 }, { "showSeller", true } };

            var expectedSql = @"
-------------------------------
-- Operation: MyOrder
-------------------------------

SELECT

  -- order (t1)
  JSON_QUERY ((
    SELECT
      t1.[Id] AS [id]

      -- order.seller (t2)
    , JSON_QUERY ((
        SELECT
          t2.[Name] AS [name]
        FROM [Seller] t2
        WHERE t1.[SellerName] = t2.[Name]
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [seller]
    FROM [Order] t1
    WHERE t1.[Id] = @id
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [order]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "id", 10 } };

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void DirectiveInclude_FalseTest()
        {
            var graphql = @"
query MyOrder($id: Int!, $showSeller: Boolean!) {
  order (id: $id) {
    id
    seller @include(if: $showSeller) { name }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "id", 10 }, { "showSeller", false } };

            var expectedSql = @"
-------------------------------
-- Operation: MyOrder
-------------------------------

SELECT

  -- order (t1)
  JSON_QUERY ((
    SELECT
      t1.[Id] AS [id]
    FROM [Order] t1
    WHERE t1.[Id] = @id
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [order]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "id", 10 } };

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void DirectiveSkip_FalseTest()
        {
            var graphql = @"
query MyOrder($id: Int!, $skipSeller: Boolean!) {
  order (id: $id) {
    id
    seller @skip(if: $skipSeller) { name }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "id", 10 }, { "skipSeller", false } };

            var expectedSql = @"
-------------------------------
-- Operation: MyOrder
-------------------------------

SELECT

  -- order (t1)
  JSON_QUERY ((
    SELECT
      t1.[Id] AS [id]

      -- order.seller (t2)
    , JSON_QUERY ((
        SELECT
          t2.[Name] AS [name]
        FROM [Seller] t2
        WHERE t1.[SellerName] = t2.[Name]
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [seller]
    FROM [Order] t1
    WHERE t1.[Id] = @id
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [order]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "id", 10 } };

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void DirectiveSkip_TrueTest()
        {
            var graphql = @"
query MyOrder($id: Int!, $skipSeller: Boolean!) {
  order (id: $id) {
    id
    seller @skip(if: $skipSeller) { name }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "id", 10 }, { "skipSeller", true } };

            var expectedSql = @"
-------------------------------
-- Operation: MyOrder
-------------------------------

SELECT

  -- order (t1)
  JSON_QUERY ((
    SELECT
      t1.[Id] AS [id]
    FROM [Order] t1
    WHERE t1.[Id] = @id
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [order]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "id", 10 } };

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void DirectiveInFragmentInclude_FalseTest()
        {
            var graphql = @"
{
  orders (first: 10) { id ... frag }
}
fragment frag on Order { date @include(if: false) shipping }
".Trim();

            var expectedSql = @"
SELECT

  -- orders (t1)
  JSON_QUERY ((
    SELECT
      t1.[Id] AS [id]
    , t1.[Shipping] AS [shipping]
    FROM [Order] t1
    ORDER BY t1.[Id]
    OFFSET 0 ROWS
    FETCH FIRST 10 ROWS ONLY
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [orders]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }
    }
}
