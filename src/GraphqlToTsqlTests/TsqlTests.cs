using DemoEntities;
using GraphqlToTsql.Translator;
using GraphqlToTsql.Util;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Translator.ValueType;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class TsqlTests : IntegrationTestBase
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
            const string graphql = "{ sellerBadges (offset: 10, first: 2) { badgeName } }";

            var expectedSql = @"
SELECT

  -- sellerBadges (t1)
  JSON_QUERY ((
    SELECT
      t1.[BadgeName] AS [badgeName]
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
            const string graphql = "{ sellers (offset: 10, first: 2, city: \"Kona\") { distributorName } }";

            var expectedSql = @"
SELECT

  -- sellers (t1)
  JSON_QUERY ((
    SELECT
      t1.[DistributorName] AS [distributorName]
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

        private void Check(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            string expectedSql,
            Dictionary<string, object> expectedTsqlParameters)
        {
            var result = Translate(graphql, graphqlParameters);

            // Show the difference between Expected and Actual Tsql
            expectedSql = expectedSql.TrimEnd();
            var actualSql = result.Tsql.TrimEnd();
            var expectedLines = expectedSql.Split('\n');
            var actualLines = (actualSql ?? "").Split('\n');
            var errorShown = false;

            for (var i = 0; i < Math.Max(expectedLines.Length, actualLines.Length); i++)
            {
                var line1 = i < expectedLines.Length ? expectedLines[i].TrimEnd() : "";
                var line2 = i < actualLines.Length ? actualLines[i].TrimEnd() : "";
                Console.WriteLine(line2);
                if (!errorShown && line1 != line2)
                {
                    var diff = 0;
                    while (diff < line1.Length - 1 && diff < line2.Length - 1 && line1[diff] == line2[diff])
                    {
                        diff++;
                    }
                    var arrow = "".PadRight(diff, '~') + '^';
                    Console.WriteLine(arrow);
                    errorShown = true;
                }
            }
            if (errorShown)
            {
                Assert.Fail("Unexpected Sql result");
            }

            // Show the difference between Expected and Actual TsqlParameters
            var actualTsqlParameters = result.TsqlParameters;
            Console.WriteLine("");
            Console.WriteLine(JsonConvert.SerializeObject(actualTsqlParameters, Formatting.Indented));
            var errorCount = 0;
            foreach (var kv in expectedTsqlParameters)
            {
                if (!actualTsqlParameters.ContainsKey(kv.Key))
                {
                    Console.WriteLine($"Expected Tsql Parameter is missing: {kv.Key} = {kv.Value}");
                    errorCount++;
                }
                else if (kv.Value?.ToString() != actualTsqlParameters[kv.Key]?.ToString())
                {
                    Console.WriteLine($"Tsql Parameter has incorrect value, parameter [{kv.Key}], expected [{kv.Value}], actual [{actualTsqlParameters[kv.Key]}]");
                    errorCount++;
                }
            }
            foreach (var kv in actualTsqlParameters)
            {
                if (!expectedTsqlParameters.ContainsKey(kv.Key))
                {
                    Console.WriteLine($"Unexpected Tsql Parameter: {kv.Key} = {kv.Value}");
                    errorCount++;
                }
            }
            Assert.IsTrue(errorCount == 0, $"{errorCount} Tsql Parameter errors");
        }

        private TsqlResult Translate(string graphql, Dictionary<string, object> graphqlParameters)
        {
            var parser = GetService<IParser>();
            var parseResult = parser.ParseGraphql(graphql, graphqlParameters, DemoEntityList.All());
            Assert.IsNull(parseResult.ParseError, $"Parse failed: {parseResult.ParseError}");

            var tsqlBuilder = GetService<ITsqlBuilder>();
            var tsqlResult = tsqlBuilder.Build(parseResult);
            Assert.IsNull(tsqlResult.TsqlError, $"TSQL generation failed: {tsqlResult.TsqlError}");

            return tsqlResult;
        }
    }
}
