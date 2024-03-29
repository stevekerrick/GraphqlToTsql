GraphqlToTsql is a .NET component that translates [GraphQL](https://graphql.org/)
queries into native [T-SQL](https://en.wikipedia.org/wiki/Transact-SQL).

It turns this...

```graphql
query hammerQuery ($name: String) {
  hammer: product (name: $name) {
    name
    price
    orderDetails {
      orderId
      quantity
      order {
        date
        seller {
          city
          state
          distributor { 
            name
          }
        }
      }
    }
  }
}
```

Into this...

```sql
-------------------------------
-- Operation: hammerQuery
-------------------------------

SELECT

  -- hammer (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    , t1.[Price] AS [price]

      -- hammer.orderDetails (t2)
    , JSON_QUERY ((
        SELECT
          t2.[OrderId] AS [orderId]
        , t2.[Quantity] AS [quantity]

          -- hammer.orderDetails.order (t3)
        , JSON_QUERY ((
            SELECT
              t3.[Date] AS [date]

              -- hammer.orderDetails.order.seller (t4)
            , JSON_QUERY ((
                SELECT
                  t4.[City] AS [city]
                , t4.[State] AS [state]

                  -- hammer.orderDetails.order.seller.distributor (t5)
                , JSON_QUERY ((
                    SELECT
                      t5.[Name] AS [name]
                    FROM [Seller] t5
                    WHERE t4.[DistributorName] = t5.[Name]
                    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [distributor]
                FROM [Seller] t4
                WHERE t3.[SellerName] = t4.[Name]
                FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [seller]
            FROM [Order] t3
            WHERE t2.[OrderId] = t3.[Id]
            FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [order]
        FROM [OrderDetail] t2
        WHERE t1.[Name] = t2.[ProductName]
        FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [orderDetails]
    FROM [Product] t1
    WHERE t1.[Name] = @name
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [hammer]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
```

See the project documentation: https://graphqltotsql.net/
