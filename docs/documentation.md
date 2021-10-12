---
layout: topicsPage
title: Documentation
---

<div markdown="1">

# How to Use GraphqlToTsql

`GraphqlToTsql` is a component that translates a GraphQL query into a
comprehensive T-SQL query, and (optionally) sends it to a SQL Server or
AzureSQL database.

## Setup

The main setup steps are covered on the [Getting Started page]({{ 'gettingStarted' | relative_url }}).
If you haven't yet visited that page, take a minute and skim its topics:
* [Get GraphqlToTsql]({{ 'gettingStarted?topic=get-graphqltotsql' | relative_url }})
* [Create Entity Mapping]({{ 'gettingStarted?topic=create-entity-mapping' | relative_url }})
* [Create Entity List]({{ 'gettingStarted?topic=create-entity-list' | relative_url }})
* [Register GraphqlActions]({{ 'gettingStarted?topic=register-graphqlactions' | relative_url }})
* [Wire Up the API]({{ 'gettingStarted?topic=wire-up-the-api' | relative_url }})
* [Two Ways to Run a Query]({{ 'gettingStarted?topic=two-ways-to-run-a-query' | relative_url }})

## Settings

When calling on `GraphqlToTsql`, there is a required parameter of type
`GraphqlActionSettings` which has the following properties.

```csharp
public class GraphqlActionSettings
{
    public bool AllowIntrospection { get; set; }
    public string ConnectionString { get; set; }
    public EmptySetBehavior EmptySetBehavior { get; set; }
    public List<EntityBase> EntityList { get; set; }
}
```

### AllowIntrospection

```csharp
public bool AllowIntrospection { get; set; }
```

[Introspection](https://graphql.org/learn/introspection/) is an important part of `GraphQL`.
It is a way of using `GraphQL` queries to discover the *kind* of data
that is available. For example, the Introspection query below finds all the Types (entities)
and the names and types of all their fields.

```graphql
{
  __schema {
    types {
      name
      fields {
      name
      type {
          name
          kind
      }
    }
  }
}
```

`GraphqlToTsql` supports Introspection queries, but you might not want to allow them because:
* They add overhead.
* A "Best Practice" is to allow Introspection in test environments so that people can
experiment with your API using a tool like [GraphiQL](https://github.com/graphql/graphiql),
but not to allow Introspection in Production.

### ConnectionString

```csharp
public string ConnectionString { get; set; }
```

The connection string to your SQL Server or Azure SQL database.

### EmptySetBehavior (Optional)

```csharp
public EmptySetBehavior EmptySetBehavior { get; set; }
```

If a GraphQL query (or part of a query) is supposed to return a list,
how should the JSON look if the list is empty?
By default, the empty list will appear in the resulting JSON
as `null`. But if you prefer, you can have it appear in the JSON
as an empty array, `[]`.

```csharp
public enum EmptySetBehavior
{
    Null = 0,
    EmptyArray
}
```

For example, assume that the `Seller` named "Zeus" has never had any orders.
This GraphQL query would generate an empty list.

```graphql
{
    seller (name: "Zeus") {
        orders {
            id
            date
        }
    }
}
```

With the default setting, `EmptySetBehavior = EmptySetBehavior.Null`,
the resulting JSON is:

```json
{
    "seller": {
        "orders": null
    }
}
```

With `EmptySetBehavior = EmptySetBehavior.EmptyArray`,
the resulting JSON is:

```json
{
    "seller": {
        "orders": []
    }
}
```

### EntityList

```csharp
public List<EntityBase> EntityList { get; set; }
```

The list of entities that are mapped to your database.

Each entity in the list is an *Instance* of an entity. The typical pattern
is described on our 
[Getting Started Page]({{ 'gettingStarted?topic=create-entity-list' | relative_url }}).

</div>

<div markdown="1">

# Entity Mapping Basics

A GraphQL query operates on interconnected *types*, each of which has
a set of strongly-typed *fields*. In `GraphqlToTsql` you create an
*Entity* for each GraphQL *type*.

Have a look at the sample `OrderEntity` class, which
maps to a database table named `Order`.

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

## EntityBase

Each of your Entities will inherit from [EntityBase.cs](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/GraphqlToTsql/Entities/EntityBase.cs).

Here are the parts of EntityBase that you need to care about.

```csharp
public abstract class EntityBase
{
    public abstract string Name { get; }
    public virtual string PluralName => $"{Name}s";
    public abstract string DbTableName { get; }
    public virtual string EntityType => DbTableName;
    public abstract string[] PrimaryKeyFieldNames { get; }
    public virtual string SqlDefinition { get; }
    public virtual long? MaxPageSize { get; }
    protected abstract List<Field> BuildFieldList();
}
```

### Name (Required)

The `Name` to use for this entity in the `GraphQL` queries. Must be singular,
and start with a lower-case letter.
It is common to give your entity the same name as the underlying database table
(but lower-cased).

```csharp
public override string Name => "order";
```

```graphql
query { order (id: 100023) { id date } }
```

### PluralName (Optional)

When querying a list of items, `GraphqlToTsql` uses a plural form of the
`Name`. By default `GraphqlToSql` appends an "s" to the `Name`. For `Name`s
where that doesn't work, you need to supply the `PluralName`.

```csharp
public override string Name => "butterfly";
public override string PluralName => "butterflies";
```

```graphql
{ butterflies { genus species } }
```

### DbTableName (Required)

The name of the database table this entity maps to.

```csharp
public override string DbTableName => "Order";
```

Note: sometimes you might map an entity
to a SQL query rather than to a physical table. You are still required
to define a `DbTableName` (the generated `T-SQL` uses it), but you can choose any name you want.

### EntityType (Optional)

The `GraphQL` Type name for the entity. The Type name is used in `GraphQL` queries that 
use [Fragments](https://graphql.org/learn/queries/#fragments).
It is also the name you will see in Introspection queries.

If you choose not to set `EntityType`, it defaults to the same as the `DbTableName`.

```csharp
public override string EntityType => "OrderType";
```

```graphql
# This query demonstrates the use of a GraphQL fragment.
# The ... is the GraphQL syntax for "use fragment".
# Notice that the fragment is strongly typed.

{
  o1: order (id: 1122) { ... orderFrag }
  o2: order (id: 3344) { ... orderFrag }
}

fragment orderFrag on OrderType {
  id
  date
  seller {
    name
    city
  }
}
```

### PrimaryKeyFieldNames (Required)

The names of the Primary Key fields. Use the GraphQL names, not the SQL column names.

You must provide a non-empty array of field names. It is needed for
`GraphQL` queries that use paging.

```csharp
public override string[] PrimaryKeyFieldNames => new[] { "orderId" };
```

See: [Paging]({{ 'documentation?topic=paging' | relative_url }})

### SqlDefinition (Optional)

Most of the time you'll map an entity to a database table.
But sometimes you want more flexibility. `GraphqlToTsql` will let
you map an entity to a `SQL SELECT` statement.

In all other ways the entity will be like other entities. You still
specify a DbTableName -- `GraphqlToTsql` needs it for the SQL
it generates.

`GraphqlToTsql` uses your `SQL SELECT` as a
[Common Table Expression](https://docs.microsoft.com/en-us/sql/t-sql/queries/with-common-table-expression-transact-sql)
in the `T-SQL` query it constructs.

For example, here is an entity from our [reference application](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/DemoEntities/SellerTotalEntity.cs).
The `SellerTotalEntity` has calculated order totals for each `Seller`.

```csharp
public class SellerTotalEntity : EntityBase
{
    public static SellerTotalEntity Instance = new SellerTotalEntity();

    public override string Name => "sellerTotal";
    public override string DbTableName => "SellerTotal";
    public override string[] PrimaryKeyFieldNames => new[] { "sellerName" };
    public override string SqlDefinition => @"
SELECT
  s.[Name] AS SellerName
, COUNT(DISTINCT o.Id) AS TotalOrders
, SUM(od.Quantity) AS TotalQuantity
, SUM(od.Quantity * p.Price) AS TotalAmount
FROM Seller s
INNER JOIN [Order] o
  ON s.Name = o.SellerName
INNER JOIN OrderDetail od
  ON o.Id = od.OrderId
INNER JOIN Product p
  ON od.ProductName = p.[Name]
GROUP BY s.[Name]
".Trim();

    protected override List<Field> BuildFieldList()
    {
        return new List<Field>
        {
            Field.Column(this, "sellerName", "SellerName", ValueType.String, IsNullable.No),
            Field.Column(this, "totalOrders", "TotalOrders", ValueType.Int, IsNullable.No),
            Field.Column(this, "totalQuantity", "TotalQuantity", ValueType.Int, IsNullable.No),
            Field.Column(this, "totalAmount", "TotalAmount", ValueType.Float, IsNullable.No),

            Field.Row(SellerEntity.Instance, "seller", new Join(
                ()=>this.GetField("sellerName"),
                ()=>SellerEntity.Instance.GetField("name"))
            )
        };
    }
}
```

### MaxPageSize (Optional)

Some of your database tables probably have more data than can reasonably be returned
in a single query. You can require incoming GraphQL queries to use paging for an entity
by setting `MaxPageSize`. For example, if you want to limit the number of `Order` 
rows that can be returned in a single query to 1000:

```csharp
public override long? MaxPageSize => 1000L;
```

The `GraphQL` query will then be required to use paging for all `Order` lists.
If the query doesn't use paging `GraphqlToTsql` will return an error.

```graphql
# These queries use "offset paging" to receive only 100 rows at a time
{ 
  orders (offset: 900, first: 100) {
    id 
    date
  } 
  sellers { 
    name 
    orders (first: 100) { 
      id 
      date 
    } 
  }
}
```

`GraphqlToTsql` supports both offset and cursor-based paging.
See [Paging]({{ 'documentation?topic=paging' | relative_url }}) for details.

### BuildFieldList() (Required)

Implement method `BuildFieldList` to define the fields for your entity.
Very often a field will map to a database column, or to a
related entity. Sometimes a field will map to a calculated value
or set of values.

See [Field Mapping]({{ 'documentation?topic=field-mapping' | relative_url }})
for details.

Here's a sneak peek at a typical `BuildFieldList` implementation.

```csharp
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

        Field.Set(OrderDetailEntity.Instance, "orderDetails", new Join(
            ()=>this.GetField("id"),
            ()=>OrderDetailEntity.Instance.GetField("orderId"))
        )
    };
}
```

</div>

<div markdown="1">

# Field Mapping

When you are defining an entity, the fields for the entity are
mapped in the `BuildFieldList` method. `GraphqlToTsql` supports six types of mapping.
* Mapping to a database Column
* Mapping to a related database row
* Mapping to a set of related database rows
* Mapping to a calculated value
* Mapping to a calculated database row
* Mapping to a calculated set of database rows

## Mapping to a Column

Mapping to a Column is the most common mapping. It defines an entity field that maps
to a database column.

To create a Column Mapping, use the static method `Field.Column()`.

```csharp
public static Field Column (
    EntityBase entity,
    string name,
    string dbColumnName,
    ValueType valueType,
    IsNullable isNullable,
    Visibility visibility = Visibility.Normal
);
```

```csharp
// Examples of Column Mappings
Field.Column(this, "name", "Name", ValueType.String, IsNullable.No)
Field.Column(this, "zip", "PostalCode", ValueType.String, IsNullable.Yes)
Field.Column(this, "quantity", "Quantity", ValueType.Int, IsNullable.No)
Field.Column(this, "shipping", "ShippingAmount", ValueType.Float, IsNullable.No)
Field.Column(this, "sellerId", "SellerId", ValueType.Int, IsNullable.No, Visibility.Hidden)
```

### EntityBase entity (Required)

The entity instance this field belongs to. Since you do field setup in the
entity's `BuildFieldList()` method, you will always pass the value `this`.

### string name (Required)

The name of the field in the GraphQL. It should begin with a lower-case letter,
since that is the convention in GraphQL.

Often you will give your field the same name as the database column it maps to,
converted to [lower camel case](https://en.wikipedia.org/wiki/Camel_case).

### string dbColumnName (Required)

The column name in the database. This column must be part of the database table
that the entity maps to.

### ValueType valueType (Required)

GraphQL has a small set of scalar value types, and these are the types
you specify in the entity mapping. Not surprisingly they align with standard
JSON types.

```csharp
ValueType.String
ValueType.Int
ValueType.Float
ValueType.Boolean
```

If the database column is type `bit`, use `ValueType.Boolean`.

If the database column is type `tinyint`, `smallint`, `int`, or `bigint`, use `ValueType.Int`.

If the database column is any other numeric, use `ValueType.Float`.

In all other cases, use `ValueType.String`.

### IsNullable isNullable (Required)

To validate arguments and variables in the `GraphQL` you need to indicate whether the
database column is nullable.

Use one of these values.

```csharp
IsNullable.Yes
IsNullable.No
```

### Visibility visibility (Optional)

You need to create Column Mappings for the primary keys on all your entities.
They're needed for mapping table joins, and for paging. But you can *hide*
those mappings from the `GraphQL` if you don't want to share your ID's
with the world.

```csharp
Visibility.Normal
Visibility.Hidden
```

This is an optional parameter in `Field.Column()`. The default is `Visibility.Normal`.

## Mapping to a Related Row

If you are mapping a database table that has a foreign key to a related table,
you can map that relationship using the static method `Field.Row`.

```csharp
public static Field Row(
    EntityBase entity,
    string name,
    Join join);
```

For example, consider the database tables `Order` and `OrderDetail`.
`OrderDetail.OrderId` is a foreign key to `Order.Id`, so when you create the
`OrderDetail` entity you'll use a Row Mapping to link to the `Order` entity.

```sql
CREATE TABLE [Order] (
    Id            INT NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED
,   SellerName    NVARCHAR(64) NOT NULL
,   [Date]        DATE NOT NULL
,   Shipping      DECIMAL(5,2) NOT NULL
,   CONSTRAINT FK_Order_Seller FOREIGN KEY (SellerName) REFERENCES Seller ([Name])
);

CREATE TABLE OrderDetail (
    OrderId       INT NOT NULL
,   ProductName   NVARCHAR(64) NOT NULL
,   Quantity      INT NOT NULL
,   CONSTRAINT PK_OrderDetail PRIMARY KEY NONCLUSTERED (OrderId, ProductName)
,   CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderId) REFERENCES [Order] (Id)
,   CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (ProductName) REFERENCES Product ([Name])
);
```

```csharp
public class OrderDetailEntity : EntityBase
{
    public static OrderDetailEntity Instance = new OrderDetailEntity();

    public override string Name => "orderDetail";
    public override string DbTableName => "OrderDetail";
    public override string[] PrimaryKeyFieldNames => new[] { "orderId", "productName" };

    protected override List<Field> BuildFieldList()
    {
        return new List<Field>
        {
            Field.Column(this, "orderId", "OrderId", ValueType.Int, IsNullable.No),
            Field.Column(this, "productName", "ProductName", ValueType.String, IsNullable.No, Visibility.Hidden),
            Field.Column(this, "quantity", "Quantity", ValueType.Int, IsNullable.No),

            Field.Row(OrderEntity.Instance, "order", new Join(
                ()=>this.GetField("orderId"),
                ()=>OrderEntity.Instance.GetField("id"))
            ),
            Field.Row(ProductEntity.Instance, "product", new Join(
                ()=>this.GetField("productName"),
                ()=>ProductEntity.Instance.GetField("name"))
            )
        };
    }
}
```

Notice in the `OrderDetailEntity` above where the `OrderEntity` is mapped
using a Row Mapping.

```csharp
Field.Row(OrderEntity.Instance, "order", new Join(
    ()=>this.GetField("orderId"),
    ()=>OrderEntity.Instance.GetField("id"))
),
```

### EntityBase entity (Required)

The singleton instance of the *related* entity. In the example above, `OrderEntity.Instance`.

### string name (Required)

The name of the row in the GraphQL. It should begin with a lower-case letter,
since that is the convention in GraphQL.

Often you will give your row the same name as the entity it maps to,
converted to [lower camel case](https://en.wikipedia.org/wiki/Camel_case).

### Join join (Required)

Indicate how the two entities are to be joined, by specifying the parent entity field
and the child entity field.

The *parent entity* is the entity you're currently mapping *from*. The *child entity* is the
entity the `Field.Row` is mapping *to*.

```csharp
public Join(Func<Field> parentFieldFunc, Func<Field> childFieldFunc)
{
    ParentFieldFunc = parentFieldFunc;
    ChildFieldFunc = childFieldFunc;
}
```

You'll notice that `Join` won't work for tables that have a compound primary key.
If your database has compound keys (or some other complicated relationship),
you'll need to use the `Calcuated Row` mapping instead.

## Mapping to a Related Set

If you are mapping a database table to a child table with a one-to-many
relationship, you will use
the static method `Field.Set` to configure the relationship.

```csharp
public static Field Set(
    EntityBase entity,
    string name,
    Join join);
```

For example, consider the same database tables `Order` and `OrderDetail`
that we used in the `Row` mapping of `OrderDetail` => `Order`.
We will now use `Field.Set` to map the reverse -- the one-to-many relationship
`Order` => `OrderDetail`s.


```csharp
public class OrderEntity : EntityBase
{
    public static OrderEntity Instance = new OrderEntity();

    public override string Name => "order";
    public override string DbTableName => "Order";
    public override string[] PrimaryKeyFieldNames => new[] { "id" };

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

            Field.Set(OrderDetailEntity.Instance, "orderDetails", new Join(
                ()=>this.GetField("id"),
                ()=>OrderDetailEntity.Instance.GetField("orderId"))
            )
        };
    }
}
```

### EntityBase entity (Required)

The singleton instance of the *related* entity. Notice in the `Field.Set` code above the entity
is set to `OrderDetailEntity.Instance`.

### string name (Required)

The name to use in the GraphQL for the child collection. It should begin with a lower-case letter,
since that is the convention in GraphQL, and it should be plural.

### Join join (Required)

Indicate how the two entities are to be joined, by specifying the parent entity field
and the child entity field.

The *parent entity* is the entity you're currently mapping. The *child entity* is the
entity the `Field.Row` is mapping *to*.

In the example above, `OrderEntity`'s `id` field joins to `OrderDetailEntity`'s `orderId` field.

Just as explained for `Row` mapping, the Join is expressed as a pair of `Func<Field>`s.
If your tables are related in a more complicated way, then `Field.Row` won't work for you --
you'll need to use the `Calculated Set` mapping explained below.

## Mapping to a Calculated Value

You can create a scalar field in your entity that isn't mapped to a database column,
but rather is defined by a SQL expression (even a complicated one.)

To create a `Calculated Field` Mapping, use the static method `Field.CalculatedField()`.
It is similar to `Field.Column()`, except that instead of specifying a database column
name you provide a SQL template.

```csharp
public static Field CalculatedField(
    EntityBase entity,
    string name,
    ValueType valueType,
    IsNullable isNullable,
    Func<string, string> templateFunc,
    Visibility visibility = Visibility.Normal
);
```

For example, in the `OrderEntity` that appeared earlier in this topic we could add
a `Calculated Field` for the `totalQuantity` on the order.

```csharp
Field.CalculatedField(this, "totalQuantity", ValueType.Int, IsNullable.No,
    (tableAlias) => $"SELECT SUM(od.Quantity) FROM OrderDetail od WHERE {tableAlias}.Id = od.OrderId"
)
```

### EntityBase entity (Required)

The entity instance this field belongs to. Since you do field setup in the
entity's `BuildFieldList()` method, you will always pass the value `this`.

### string name (Required)

The name of the field in the GraphQL. It should begin with a lower-case letter.

### ValueType valueType (Required)

GraphQL has a small set of scalar value types, and these are the types
you specify in the entity mapping. Not surprisingly they align with standard
JSON types.

```csharp
ValueType.String
ValueType.Int
ValueType.Float
ValueType.Boolean
```

If your SQL expression yields a value of type `bit`, use `ValueType.Boolean`.

If your SQL expression yields a value of type `tinyint`, `smallint`, `int`, or `bigint`, use `ValueType.Int`.

If your SQL expression yields a type of any other numeric, use `ValueType.Float`.

In all other cases, use `ValueType.String`.

### IsNullable isNullable (Required)

Indicate whether your SQL expression yields a value that can be null.

Use one of these values.

```csharp
IsNullable.Yes
IsNullable.No
```

### Func<string, string> templateFunc (Required)

Template function to generate a SQL expression for the field.
The function has a single argument, representing the table alias
`GraphqlToTsql` has assigned to the entity's table.

This is one of the most flexible capabilities in `GraphqlToTsql`,
and allows you to expose your data in ways that
don't match the physical database schema.

Hopefully the examples below will help make it clear. :-)

### Visibility visibility (Optional)

This is an optional parameter in `Field.CalculatedField()`. The default is `Visibility.Normal`.

If you don't want to expose your `Calculated Field` in the `GraphQL` you can set the
visibility to `Visibility.Hidden`. `Visibility.Hidden` is normally only used to hide Id
columns, but it's available on `Calculated Fields` if you need it.

```csharp
Visibility.Normal
Visibility.Hidden
```

### Calculated Value example 1: TotalQuantity

Let's calculate the `TotalQuantity` for an order. The mapping looks
like this.

```csharp
Field.CalculatedField(this, "totalQuantity", ValueType.Int, IsNullable.No,
    (tableAlias) => $"SELECT SUM(od.Quantity) FROM OrderDetail od WHERE {tableAlias}.Id = od.OrderId"
)
```

It's used in GraphQL like this.

```graphql
query ($orderId: Int) {
    order (id: $orderId) {
        id
        totalQuantity
    }
}
```

And here is the complete T-SQL that `GraphqlToSql` generates for the query.

```sql
SELECT

  -- order (t1)
  JSON_QUERY ((
    SELECT
      t1.[Id] AS [id]
    , (SELECT SUM(od.Quantity) FROM OrderDetail od WHERE t1.Id = od.OrderId) AS [totalQuantity]
    FROM [Order] t1
    WHERE t1.[Id] = @orderId
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [order]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
```

Using the data in our
[Demo App]({{ 'demo' | relative_url }}),
it results in this JSON.

```json
{
  "order": {
    "id": 1,
    "totalQuantity": 1
  }
}
```

### Calculated Value example 2: FormattedDate

In the same `OrderEntity`, we can expose the `OrderDate` in a custom format.

```csharp
Field.CalculatedField(this, "formattedDate", ValueType.String, IsNullable.No,
    (tableAlias) => $"FORMAT({tableAlias}.[Date], 'dd/MM/yyyy', 'en-US' )"
),
```

```graphql
{
    order (id: 1) {
        formattedDate
    }
}
```

Here is the T-SQL that `GraphqlToTsql` generated, and the resulting data.

```sql
SELECT

  -- order (t1)
  JSON_QUERY ((
    SELECT
      (FORMAT(t1.[Date], 'dd/MM/yyyy', 'en-US' )) AS [formattedDate]
    FROM [Order] t1
    WHERE t1.[Id] = @id
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [order]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
```

```json
{
  "order": {
    "formattedDate": "01/01/2020"
  }
}
```

## Mapping to a Calculated Row

`Calculated Row` mapping is similar to the regular `Row` mapping, but is
much more flexible.

You'll recall that `Row` mapping allows you to define a field that is a
single row, when your parent entity holds a regular `Foreign Key` to the child
entity.

In a `Calculated Row` mapping you write a custom `SQL SELECT` statement to retrieve
the child row. You declare the mapping using the `Field.CalculatedRow` static method.

```csharp
public static Field CalculatedRow(
    EntityBase entity,
    string name,
    Func<string, string> templateFunc
);
```

### EntityBase entity (Required)

The singleton instance of the *related* entity.

### string name (Required)

The name of the row in the GraphQL. It should begin with a lower-case letter,
since that is the convention in GraphQL.

### Func<string, string> templateFunc (Required)

Template function to generate a `SQL SELECT` for the field.
The function has a single argument, representing the table alias
`GraphqlToTsql` has assigned to the entity's table.

### Calculated Row example: mostRecentOrder

Let's find the most recent order for a seller. Here is a trimmed-down mapping for the
`SellerEntity`. The `CalculatedRow` mapping for `mostRecentOrder` appears at the bottom.

```csharp
public class SellerEntity : EntityBase
{
    public static SellerEntity Instance = new SellerEntity();

    public override string Name => "seller";
    public override string DbTableName => "Seller";
    public override string[] PrimaryKeyFieldNames => new[] { "name" };

    protected override List<Field> BuildFieldList()
    {
        return new List<Field>
        {
            Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
            ...

            Field.Set(OrderEntity.Instance, "orders", new Join(
                ()=>this.GetField("name"),
                ()=>OrderEntity.Instance.GetField("sellerName"))
            ),

            Field.CalculatedRow(OrderEntity.Instance, "mostRecentOrder",
                (tableAlias) => $@"SELECT TOP 1 *
FROM [Order]
WHERE {tableAlias}.Name = [Order].SellerName
ORDER BY [Order].[Date] DESC"
            )
        };
    }
}
```

It's used in GraphQL like this.

```graphql
{
    seller (name: "Donada") {
        mostRecentOrder { id date }
    }
}
```

And here is the complete T-SQL that `GraphqlToSql` generates for the query.

```sql

SELECT
  -- seller (t1)
  JSON_QUERY ((
    SELECT

      -- seller.mostRecentOrder (t2)
      JSON_QUERY ((
        SELECT
          t2.[Id] AS [id]
        , t2.[Date] AS [date]
        FROM (SELECT TOP 1 *
FROM [Order]
WHERE t1.Name = [Order].SellerName
ORDER BY [Order].[Date] DESC) t2
        FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [mostRecentOrder]
    FROM [Seller] t1
    WHERE t1.[Name] = @name
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [seller]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
```

Using the data in our
[Demo App]({{ 'demo' | relative_url }}),
it results in this JSON.

```json
{
  "seller": {
    "mostRecentOrder": {
      "id": 12,
      "date": "2020-05-19"
    }
  }
}
```

## Mapping to a Calculated Set

`Calculated Set` mapping is similar to the regular `Set` mapping, but is
much more flexible.

You'll recall that `Set` mapping allows you to define a field that is a
*list of rows*, and is normally used when the child entity you're mapping to
holds a `Foreign Key` to the parent entity.

In a `Calculated Set` mapping you write a custom `SQL SELECT` statement to retrieve
a set of child rows. You declare the mapping using the `Field.CalculatedSet` static method.

```csharp
public static Field CalculatedSet(
    EntityBase entity,
    string name,
    Func<string, string> templateFunc
);
```

### EntityBase entity (Required)

The singleton instance of the *related* entity.

### string name (Required)

The name of the set in the GraphQL. It should begin with a lower-case letter,
and it should be plural.

### Func<string, string> templateFunc (Required)

Template function to generate a `SQL SELECT` for the field.
The function has a single argument, representing the table alias
`GraphqlToTsql` has assigned to the entity's table.

### Calculated Set example 1: Product.sellers

Let's map a field on `ProductEntity` to show all the sellers that have ever sold the
product. Here is a trimmed-down mapping for the
`ProductEntity`. The `CalculatedSet` mapping for `sellers` appears at the bottom.

```csharp
public class ProductEntity : EntityBase
{
    public static ProductEntity Instance = new ProductEntity();

    public override string Name => "product";
    public override string DbTableName => "Product";
    public override string[] PrimaryKeyFieldNames => new[] { "name" };

    protected override List<Field> BuildFieldList()
    {
        return new List<Field>
        {
            Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
            ...

            Field.CalculatedSet(SellerEntity.Instance, "sellers",
                (tableAlias) => $@"SELECT DISTINCT s.*
FROM OrderDetail od
INNER JOIN [Order] o ON od.OrderId = o.Id
INNER JOIN Seller s ON o.SellerName = s.Name
WHERE {tableAlias}.Name = od.ProductName"
            )
        };
    }
}
```

It's used in GraphQL like this.

```graphql
{
    product (name: "Pliers") {
        sellers { name }
    }
}
```

And here is the complete T-SQL that `GraphqlToSql` generates for the query.

```sql
SELECT

  -- product (t1)
  JSON_QUERY ((
    SELECT

      -- product.sellers (t2)
      JSON_QUERY ((
        SELECT
          t2.[Name] AS [name]
        FROM (SELECT DISTINCT s.*
FROM OrderDetail od
INNER JOIN [Order] o ON od.OrderId = o.Id
INNER JOIN Seller s ON o.SellerName = s.Name
WHERE t1.Name = od.ProductName) t2
        FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [sellers]
    FROM [Product] t1
    WHERE t1.[Name] = @name
    FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS [product]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
```

Using the data in our
[Demo App]({{ 'demo' | relative_url }}),
it results in this JSON.

```json
{
  "product": {
    "sellers": [
      {
        "name": "Bill"
      },
      {
        "name": "Chris"
      },
      {
        "name": "Erik"
      }
    ]
  }
}
```

### Calculated Set example 2: Seller.descendants

The SQL you use in your `Field.CalculatedSet` can make use of any of the
database's tables, views, and functions (limited by the permissions of the user
in the connection string, of course.) A common and powerful technique is to
make use of a Table-Valued Function (TVF) that encapsulates complicated
parts of the query. In this example, we use a TVF that contains a
[recursive CTE](https://docs.microsoft.com/en-us/sql/t-sql/queries/with-common-table-expression-transact-sql?view=sql-server-ver15#guidelines-for-defining-and-using-recursive-common-table-expressions). 

In the reference database there is a `Seller` table
* Keyed by `Name`
* With a `DistributorName` column that self-references the `Seller` table

The database has a TVF to find all the descendants for a distributor.
(This has been predefined in the database.)

```sql
CREATE FUNCTION tvf_AllDescendants (
  @parentName VARCHAR(64)
)
RETURNS TABLE
AS
RETURN
  WITH ParentCTE AS (
    SELECT
      [Name]
    , DistributorName
    FROM Seller s
    WHERE s.DistributorName = @parentName

    UNION ALL

    SELECT
      child.[Name]
    , child.DistributorName
    FROM ParentCTE parent
    INNER JOIN Seller child
      ON child.DistributorName = parent.[Name]
  )

  SELECT
    [Name]
  FROM ParentCTE;
GO
```

The C# code below defines a GraphQL field named `descendants`, of type `Seller[]`.

```csharp
public class SellerEntity : EntityBase
{
    ...
    protected override List<Field> BuildFieldList()
    {
        return new List<Field>
        {
            ...
            Field.CalculatedSet(this, "descendants", IsNullable.Yes,
                (tableAlias) => $"SELECT s.* FROM tvf_AllDescendants({tableAlias}.Name) d INNER JOIN Seller s ON d.Name = s.Name"
            ),
            ...
        };
    }
}
```

</div>

<div markdown="1">

# Paging

Paging is an important part of a Production-level `GraphQL` API.
In traditional `GraphQL` implementations paging can be a bit difficult
to implement. `GraphqlToTsql` makes it easier.

Paging isn't part of the formal `GraphQL` specification, but graphql.org *does*
provide [Best Practices guidance](https://graphql.org/learn/pagination/).
`GraphqlToTsql` follows their guidance, *except* that `GraphqlToTsql` does not
include a `pageInfo` object.

There are two competing techniques for implementing pagination,
offset-based pagination and cursor-based pagination. `GraphqlToTsql` supports them both.

Cursor-based pagination is rather new, and quite a bit more efficient when
querying large data sets. It isn't unique to `GraphQL`. Here's a really excellent
article from our friends at *Slack* about how they're using it:
[Evolving API Pagination at Slack](https://slack.engineering/evolving-api-pagination-at-slack).

## Sneak Peek

The paging syntax can look a bit confusing if you're new to `GraphQL`.
It is easiest to show an example, and then explain the pieces.

Here's an example of a query to retrieve the first page of
a seller's order history, using cursor-based paging.

```graphql
{
  seller (name: "Bill") {
    ordersConnection (first: 2) {
      totalCount
      edges {
        cursor # <-- here we're asking for the "cursor" for each Order
        node { # <-- here's where the Order info starts
          id
          date
          orderDetails {
            product { name }
            quantity
          }
        }
      }
    }
  }
}
```

Here's the resulting JSON data.

```json
{
  "seller": {
    "ordersConnection": {
      "totalCount": 6,
      "edges": [
        {
          "cursor": "M3wyfE9yZGVy.3cecb94d",
          "node": {
            "id": 2,
            "date": "2020-01-29",
            "orderDetails": [
              {
                "product": {
                  "name": "Hammer"
                },
                "quantity": 1
              },
              {
                "product": {
                  "name": "Pliers"
                },
                "quantity": 1
              }
            ]
          }
        },
        {
          "cursor": "M3wzfE9yZGVy.b3987a32",
          "node": {
            "id": 3,
            "date": "2020-02-06",
            "orderDetails": [
              {
                "product": {
                  "name": "Hammer"
                },
                "quantity": 3
              },
              {
                "product": {
                  "name": "Drill"
                },
                "quantity": 3
              }
            ]
          }
        }
      ]
    }
  }
}
```

And here's the query to retrieve subsequent pages.

```graphql
query sellerOrders ($cursor: String) {
  seller (name: "Bill") {
    ordersConnection (first: 2, after: $cursor) { # <-- The "after" argument is the important part
      edges {
        cursor
        node {
          id
          date
          orderDetails {
            product { name }
            quantity
          }
        }
      }
    }
  }
}
```

```json
{
  "seller": {
    "ordersConnection": {
      "edges": [
        {
          "cursor": "M3w0fE9yZGVy.b1e89f00",
          "node": {
            "id": 4,
            "date": "2020-02-11",
            "orderDetails": [
              {
                "product": {
                  "name": "Hammer"
                },
                "quantity": 1
              },
              {
                "product": {
                  "name": "Hand Saw"
                },
                "quantity": 1
              },
              {
                "product": {
                  "name": "Circular Saw"
                },
                "quantity": 1
              }
            ]
          }
        },
        {
          "cursor": "M3w1fE9yZGVy.e646b265",
          "node": {
            "id": 5,
            "date": "2020-02-14",
            "orderDetails": [
              {
                "product": {
                  "name": "Hammer"
                },
                "quantity": 1
              },
              {
                "product": {
                  "name": "Pipe Wrench"
                },
                "quantity": 3
              },
              {
                "product": {
                  "name": "Screwdriver"
                },
                "quantity": 3
              }
            ]
          }
        }
      ]
    }
  }
}
```

## Connection --> Edges --> Node

You're probably wondering what that `ordersConnection` structure is.

Though not part of the *official* `GraphQL` specification, connections are the
*officially recommended* way of exposing metadata about a list. `GraphqlToTsql`
allows `GraphQL` queries on the plain lists (e.g. `seller.orders`) and also
queries on those same lists with metadata (e.g. `seller.ordersConnection`).
Just add the word `Connection` to the end of a list's field name.

A `Connection` entity contains the *list* plus some metadata about the *list*.
In `GraphqlToTsql` a `Connection` contains two fields:
* `totalCount` - the count of items in the complete list. Even if you've requested
just one page of the list's data, the `totalCount` will be the count for the full list.
* `edges` - the list (or a page of data from the list)

The `edges` property is an array. Each item of the array contains *one of the items you're querying for*,
plus some metadata about *that item*. In `GraphqlToTsql` an `Edge` item contains two fields:
* `cursor` - an opaque identifier for the row. Think of it like a bookmark.
* `node` - the item. Even though it has the name `node`, this is exactly the same item
you can query on if you're querying plain lists instead of a `Connection`.

That's a lot to digest if you're new to cursor-based pagination. But it's an important
pattern, and it will help you understand the rest of the details.

## totalCount

You can include `totalCount` in the query to see the total number of rows
in a dataset.

Keep performance in mind. `totalCount` is not free. If you're doing a paged query only ask for
`totalCount` on the first page.

## cursor

This is *not* the same thing as a SQL cursor. In cursor-based pagination, a cursor is a
row identifier. It's used like this.

1. When you query for the first page of data, include `cursor` in the request.
For example, if you're paging through a data set 100 rows at a time your query would
have a pattern like:

    ```graphql
    {
      # ...
        xxxxConnection (first: 100) {
          edges {
            cursor
            node { # ...
            }
          }
        }
      # ...
    }
    ```

2. In subsequent requests use an `after` argument,
with the `cursor` value of the last row in the prior page.

    ```graphql
    {
      # ...
        xxxxConnection (first: 100, after: "xxxxxxx") {
          edges {
            cursor
            node { # ...
            }
          }
        }
      # ...
    }
    ```

3. Keep querying until the page comes back with fewer than 100 rows.

Cursors are designed to be *opaque*, meaning that they are encoded in such
a way that you can't see the raw data they're made from. That's partly because
it's wise to hide implementation details, and partly because consumers of
your API shouldn't try to create their own cursor values.

But there's no real magic to `cursors`. Basically they store the value of
the *Primary Key* for the row.

## Offset Paging

Most of this topic has focused on cursor-based pagination because it's
much more efficient on large datasets. But `GraphqlToTsql` supports
offset-based pagination as well.

One good thing about offset-base pagination is that you don't need
the extra `Connection / Edges / Node` syntax because you don't need
to query for `cursors`. (Though you can still query for the `totalCount` value
on the `Connection` if you want to.)

Use arguments `first` and `offset` for offset-based paging

```graphql
{
  seller (name: "Bill") {
    orders (first: 100, offset: 1100) {
      id
      date
      orderDetails {
        product { name }
        quantity
      }
    }
  }
}
```

## MaxPageSize

When you are creating an entity, one of the things you can set is `MaxPageSize`.
You *should* set `MaxPageSize` for any entity that could have more than a few
hundred rows.

If you set `MaxPageSize` you force queries to use paging anywhere
a *set* of that entity is queried.

For example, here you can see `OrderEntity` configured with a `MaxPageSize` of 100.

```csharp
public static OrderEntity Instance = new OrderEntity();

public override string Name => "order";
public override string DbTableName => "Order";
public override string[] PrimaryKeyFieldNames => new[] { "id" };
public override long? MaxPageSize => 100L;

protected override List<Field> BuildFieldList()
{
    return new List<Field>
    {
        ...
    };
}
```

All of these queries will be rejected, with the error message
> Paging is required with orders

```graphql
{ orders { id date }}
{ seller(name: "Bob") { orders { id date }}}
{ products { name description orders { date } sellers { name }}}
```

## Limitations

Cursor-based paging is not supported for tables with compound keys.
The reason stems from how cursors are implemented. When a subquery
has an argument like `after: $cursor`, the generated SQL will have
a WHERE clause like `WHERE id > 99375`. The generated SQL is very
efficient, but the approach won't work for tables with compound keys.

Offset-based paging works fine for tables with compound keys, though
it will be less efficient than cursor-based paging could be.

## Use Variables

Most of the sample `GraphQL` queries in this topic didn't use Variables, but
that was to keep the sample code as clear as possible.

Typically when doing paging in `GraphQL` you declare your paging
values as `Variables` at the beginning of your query, and when you
submit the `GraphQL` query you send in a dictionary of `Variable values`
with it.

This keeps your queries tidy, and also keeps you from having to use
string interpolation to build the `GraphQL` query.

```graphql
query sellerOrders ($name: String, $first: Int, $cursor: String) {
  seller (name: $name) {
    ordersConnection (first: $first, after: $cursor) {
      edges {
        cursor
        node {
          id
          date
          orderDetails {
            product { name }
            quantity
          }
        }
      }
    }
  }
}
```

Here are the Variables you send with the above query.

```json
{
    "name": "Bill",
    "first": 100,
    "after": "M3w0fE9yZGVy.b1e89f00"
}
```

</div>

<div markdown="1">

# Sorting

Sorting is not part of the `GraphQL` specification, but it is important if you are
using `GraphQL` to populate UI tables and grids. `GraphqlToTsql` added support
for sorting in version 1.1.

## Sort by a Single Field

To sort by a single field, use an `orderBy` argument, with a value of `fieldName`: `ASC`/`DESC`, 
e.g. `orderBy: {date: DESC}`.

Here's an example of a query to retrieve the first page of
a seller's order history, sorted descending by date. Notice that the `orderBy` argument
works alongside the paging arguments. In fact, if you specify an `orderBy` but don't
specify paging arguments, the generated `T-SQL` will sort by the primary key.

```graphql
query SellerDetails {
  seller (name: "bill") {
    name city state postalCode
    orders (orderBy: {date: DESC}, first: 1000) { id date }
  }
}
```

Here's the resulting JSON data.

```json
{
  "seller": {
    "name": "Bill",
    "city": "Los Angeles",
    "state": "CA",
    "postalCode": "90001",
    "orders": [
      {
        "id": 7,
        "date": "2020-03-12T12:12:00Z"
      },
      {
        "id": 6,
        "date": "2020-02-17T07:41:58+00:00"
      },
      {
        "id": 5,
        "date": "2020-02-14T10:10:15Z"
      },
      {
        "id": 4,
        "date": "2020-02-11T14:30:00+00:00"
      },
      {
        "id": 3,
        "date": "2020-02-06T20:12:12+00:00"
      },
      {
        "id": 2,
        "date": "2020-01-29T13:58:13+00:00"
      }
    ]
  }
}
```

## Sort by Multiple Fields

To sort by multiple fields, your `orderBy` expression needs to be an array, like
`orderBy: [{field1: ASC}, {field2: DESC}]`. You might find that surprising -- it might seem
more natural to use a single object with two properties, rather than an array of 
objects each with a single property. The reason is that the array will preserve its
order better through serialization/deserialization.

Here's a sample query that sorts by two fields.

```graphql
query SellerDetails {
  seller (name: "bill") {
    name city state postalCode
    recruits (orderBy: [{state: ASC}, {name: ASC}]) { state name }
  }
}
```

Here's the resulting JSON data.

```json
{
  "seller": {
    "name": "Bill",
    "city": "Los Angeles",
    "state": "CA",
    "postalCode": "90001",
    "recruits": [
      {
        "state": "IN",
        "name": "Donada"
      },
      {
        "state": "MI",
        "name": "Georgey"
      },
      {
        "state": "NY",
        "name": "Erik"
      },
      {
        "state": "OH",
        "name": "Francesca"
      },
      {
        "state": "OH",
        "name": "Helena"
      }
    ]
  }
}
```

## Sorting Using a Variable

If the sorted data is rendered in a table or grid, then most likely the user
will want to sort the data, and you will want to use a `GraphQL` variable
to avoid hard-coding the sorting criteria.

This example shows the use of a variable in sorting. Notice that the `GraphQL` variable type is `OrderBy`.

```graphql
query BestProduct ($order: OrderBy) {
  products (first: 1, orderBy: $order) {
    name price totalRevenue
  }
}
```

Here is the Variable sent with the above query. Since Variables are sent as JSON,
the field name and `ASC`/`DESC` have to be quoted.

```json
{
  "order": { "totalRevenue": "DESC" }
}
```

Here's the resulting JSON data.

```json
{
  "products": [
    {
      "name": "Hammer",
      "price": 29.95,
      "totalRevenue": 1527.45
    }
  ]
}
```

The above example is interesting because the sorting is being done on a calculated value.
Here's the T-SQL that was generated.

```sql
-------------------------------
-- Operation: BestProduct
-------------------------------

SELECT

  -- products (t1)
  JSON_QUERY ((
    SELECT
      t1.[Name] AS [name]
    , t1.[Price] AS [price]
    , (SELECT (SELECT SUM(od.Quantity) FROM OrderDetail od WHERE t1.[Name] = od.ProductName) * t1.Price) AS [totalRevenue]
    FROM [Product] t1
    ORDER BY (SELECT (SELECT SUM(od.Quantity) FROM OrderDetail od WHERE t1.[Name] = od.ProductName) * t1.Price) DESC, t1.[Name] DESC
    OFFSET 0 ROWS
    FETCH FIRST 1 ROWS ONLY
    FOR JSON PATH, INCLUDE_NULL_VALUES)) AS [products]

FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER;
```

## Limitations

Unfortunately at this time `GraphqlToTsql` does not support
* Using a Variable or Argument for `ASC`/`DESC`
* Sorting by a field in a joined table. For example, when retrieving `orders { id date seller { name }}`, you are not able to sort by `seller.name`.

We plan to support both of these scenarios in a future version.

</div>