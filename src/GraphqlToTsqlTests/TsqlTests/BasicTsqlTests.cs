using GraphqlToTsql.Entities;
using NUnit.Framework;
using System.Collections.Generic;

namespace GraphqlToTsqlTests.TsqlTests
{
    [TestFixture]
    public class BasicTsqlTests : TsqlTestBase
    {
        [Test]
        public void SimpleQueryTest()
        {
            const string graphql = "{ sellers { name } }";

            var expectedSql = @"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Seller] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void AliasTest()
        {
            const string graphql = "{ dealers: sellers { HappyName: name } }";

            var expectedSql = @"
SELECT

  -- dealers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [HappyName]
    FROM [Seller] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [dealers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void ArgumentTest()
        {
            const string graphql = "{ order (id: 1) { shipping } }";

            var expectedSql = @"
SELECT

  -- order (t1)
  JSON_QUERY ((
    SELECT
      t1.[Shipping] AS [shipping]
    FROM [Order] t1
    WHERE t1.[Id] = @id
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [order]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> {
                {"id", 1 }
            };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void JoinTest()
        {
            const string graphql = "{ order (id: 10) { id seller { name } } }";

            var expectedSql = @"
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

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void VariableTest()
        {
            const string graphql = "query VariableTest($nameVar: String = \"Hammer\", $priceVar: Float) { products (name: $nameVar, price: $priceVar) { price } }";
            var graphqlParameters = new Dictionary<string, object> { { "priceVar", 11.22 } };

            var expectedSql = @"
-------------------------------
-- Operation: VariableTest
-------------------------------

SELECT

  -- products (t1)
  JSON_QUERY ((
    SELECT
      t1.[Price] AS [price]
    FROM [Product] t1
    WHERE t1.[Name] = @nameVar AND t1.[Price] = @priceVar
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [products]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> {
                { "nameVar", "Hammer" },
                { "priceVar", 11.22 }
            };

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void NullVariableTest()
        {
            const string graphql = "query NullVariableTest($cityVar: String) { sellers (city: $cityVar) { name } }";
            var graphqlParameters = new Dictionary<string, object> { { "cityVar", null } };

            var expectedSql = @"
-------------------------------
-- Operation: NullVariableTest
-------------------------------

SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Seller] t1
    WHERE t1.[City] IS NULL
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, graphqlParameters, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void CalculatedFieldTest()
        {
            const string graphql = "{ products { price totalRevenue } }";

            var expectedSql = @"
SELECT

  -- products (t1)
  JSON_QUERY ((
    SELECT
      t1.[Price] AS [price]
    , (SELECT (SELECT SUM(od.Quantity) FROM OrderDetail od WHERE t1.[Name] = od.ProductName) * t1.Price) AS [totalRevenue]
    FROM [Product] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [products]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void QueryOnBoolFieldTest()
        {
            const string graphql = "{ badges (isSpecial: true) { name isSpecial } }";

            var expectedSql = @"
SELECT

  -- badges (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    , t1.[IsSpecial] AS [isSpecial]
    FROM [Badge] t1
    WHERE t1.[IsSpecial] = @isSpecial
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [badges]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "isSpecial", 1 } };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void FragmentTest()
        {
            var graphql = @"
{
  badges { ... frag}
  sellers { city }
}
fragment frag on Badge { name }
".Trim();

            var expectedSql = @"
SELECT

  -- badges (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    FROM [Badge] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [badges]

  -- sellers (t2)
, JSON_QUERY ((
    SELECT
      t2.[City] AS [city]
    FROM [Seller] t2
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void NonNullListTest()
        {
            const string graphql = "{ seller (name: \"Zeus\") { name sellerBadges { dateAwarded } } }";

            var expectedSql = @"
SELECT

  -- seller (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]

      -- seller.sellerBadges (t2)
    , ISNULL (JSON_QUERY ((
        SELECT
          t2.[DateAwarded] AS [dateAwarded]
        FROM [SellerBadge] t2
        WHERE t1.[Name] = t2.[SellerName]
        FOR JSON PATH, INCLUDE_NULL_VALUES)), '[]') AS [sellerBadges]
    FROM [Seller] t1
    WHERE t1.[Name] = @name
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [seller]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "name", "Zeus" } };

            Check(graphql, null, expectedSql, expectedTsqlParameters, EmptySetBehavior.EmptyArray);
        }

        [Test]
        public void ComplicatedQueryTest()
        {
            var graphql = @"
query hammerQuery ($name: String) {
  product1: product (name: $name) {
    name price
    orderDetails {
      orderId quantity
      order {
        date seller {
          city state
          distributor { name }
          recruits { name }
          sellerBadges { badge { name } }
        }
      }
    }
  }
}
".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "name", "Hammer" } };

            var result = Translate(graphql, graphqlParameters);
            var tsql = result.Tsql;
            Assert.IsTrue(tsql.Contains("hammerQuery"));
            Assert.IsTrue(tsql.Contains("Product"));
            Assert.IsTrue(tsql.Contains("OrderDetail"));
            Assert.IsTrue(tsql.Contains("Order"));
            Assert.IsTrue(tsql.Contains("Seller"));
            Assert.IsTrue(tsql.Contains("SellerBadge"));
            Assert.IsTrue(tsql.Contains("Badge"));
        }

        [Test]
        public void FilterTest()
        {
            const string graphql = "{ products (name: \"Hammer\") { price } }";

            var expectedSql = @"
SELECT

  -- products (t1)
  JSON_QUERY ((
    SELECT
      t1.[Price] AS [price]
    FROM [Product] t1
    WHERE t1.[Name] = @name
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [products]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "name", "Hammer" } };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void FilterOnCalculatedFieldTest()
        {
            const string graphql = "{ products (totalRevenue: 100.12) { price } }";

            var expectedSql = @"
SELECT

  -- products (t1)
  JSON_QUERY ((
    SELECT
      t1.[Price] AS [price]
    FROM [Product] t1
    WHERE (SELECT (SELECT SUM(od.Quantity) FROM OrderDetail od WHERE t1.[Name] = od.ProductName) * t1.Price) = @totalRevenue
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [products]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object> { { "totalRevenue", 100.12M } };

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void CalculatedSetTest()
        {
            const string graphql = "{ sellers { name descendants { name city } } }";

            var expectedSql = @"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]

      -- sellers.descendants (t2)
    , JSON_QUERY ((
        SELECT
          t2.[Name] AS [name]
        , t2.[City] AS [city]
        FROM (SELECT s.* FROM tvf_AllDescendants(t1.Name) d INNER JOIN Seller s ON d.Name = s.Name) t2
        FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [descendants]
    FROM [Seller] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void CalculatedRowTest()
        {
            const string graphql = "{ sellers { name apexDistributor { name city } } }";

            var expectedSql = @"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]

      -- sellers.apexDistributor (t2)
    , JSON_QUERY ((
        SELECT
          t2.[Name] AS [name]
        , t2.[City] AS [city]
        FROM (SELECT s.* FROM tvf_AllAncestors(t1.Name) d INNER JOIN Seller s ON d.Name = s.Name AND s.DistributorName IS NULL) t2
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [apexDistributor]
    FROM [Seller] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }

        [Test]
        public void VirtualTableTest()
        {
            const string graphql = "{ sellers { name sellerProductTotals { totalAmount } } }";

            var expectedSql = @"
WITH [SellerProductTotal] AS (
  SELECT
    o.SellerName
  , od.ProductName
  , SUM(od.Quantity) AS TotalQuantity
  , SUM(od.Quantity * p.Price) AS TotalAmount
  FROM OrderDetail od
  INNER JOIN [Order] o
    ON od.OrderId = o.Id
  INNER JOIN Product p
    ON od.ProductName = p.[Name]
  GROUP BY o.SellerName, od.ProductName
)

SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]

      -- sellers.sellerProductTotals (t2)
    , JSON_QUERY ((
        SELECT
          t2.[TotalAmount] AS [totalAmount]
        FROM [SellerProductTotal] t2
        WHERE t1.[Name] = t2.[SellerName]
        FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellerProductTotals]
    FROM [Seller] t1
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
".Trim();
            var expectedTsqlParameters = new Dictionary<string, object>();

            Check(graphql, null, expectedSql, expectedTsqlParameters);
        }
    }
}
