---
layout: topicsPage
title: Welcome!
---

<div markdown="1">

# What is GraphqlToTsql?

It is a .NET component that translates [GraphQL](https://graphql.org/)
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

</div>

<div markdown="1">

# Hmm, is it easy?

We all know GraphQL can be hard. GraphqlToTsql makes it a little easier.
* NO servers to install
* NO resolvers to write
* NO new technologies to add to your stack

GraphqlToTsql is a *NuGet* package, not a system or a service.
* *You* supply the API endpoint to receive GraphQL queries
* *You* supply a connection string to your SQL Server or Azure SQL database
* *You* write entity mappings
* *GraphqlToTsql* will to translate the GraphQL into a comprehensive T-SQL query
and send it to your database. You get back the data as a JSON string.

## Sounds easy, what's the catch?

Actually there are *three* catches...

1. GraphqlToTsql only works with specific technologies
    * .NET (GraphqlToSql targets .NET Standard 2.0)
    * SQL Server / Azure SQL. The T-SQL that is generated is specific to Microsoft databases.  

2. At this time, only the *query* portion of the GraphQL spec is supported. *Mutations* are not
supported.

3. You have to write entity mappings. They're easier to write than resolvers, and
they're powerful. They control what parts of your database are
available to GraphQL queries, and how things are named.
Your Entity Mappers can include custom join criteria, virtual tables,
and computed values.

To give you the idea, here's a sample entity mapping.

```csharp
public class OrderEntity : EntityBase
{
    public static OrderEntity Instance = new OrderEntity();

    public override string Name => "order";
    public override string DbTableName => "Order";
    public override string[] PrimaryKeyFieldNames => new[] { "id" };
    public override long? MaxPageSize => 1000L;

    protected override List<Field> BuildFieldList()
    {
        return new List<Field>
        {
            Field.Column(this, "id", "Id", ValueType.Int, IsNullable.No),
            Field.Column(this, "sellerName", "SellerName", ValueType.String, IsNullable.No, Visibility.Hidden),
            Field.Column(this, "date", "Date", ValueType.String, IsNullable.No),
            Field.Column(this, "shipping", "Shipping", ValueType.Float, IsNullable.No),

            Field.Row(SellerEntity.Instance, "seller", new Join(
                ()=>this.GetField("sellerName"),
                ()=>SellerEntity.Instance.GetField("name"))
            ),

            Field.Set(OrderDetailEntity.Instance, "orderDetails", IsNullable.No, new Join(
                ()=>this.GetField("id"),
                ()=>OrderDetailEntity.Instance.GetField("orderId"))
            )
        };
    }
}
```

The [Documentation page]({{ 'documentation' | relative_url }}) takes you through all the details.

</div>

<div markdown="1">

# License

GraphqlToTsql is licensed under the [MIT License](https://opensource.org/licenses/MIT).

</div>
