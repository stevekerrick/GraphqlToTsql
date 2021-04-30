---
layout: topicsPage
title: Documentation
---

<div markdown="1">

# How to Use GraphqlToTsql

`GraphqlToTsql` is a component that translates a GraphQL query into a
comprehensive TSQL query, and (optionally) sends it to a SQL Server or
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

## GraphqlActionSettings

The `GraphqlActions` class is the top-level `GraphqlToTsql` class.
It has two public methods you can use to process a GraphQL query:
* `TranslateAndRunQuery`
* `TranslateToTsql`

Both methods have a required parameter of type
`GraphqlActionSettings` which has three properties:
* AllowIntrospection
* ConnectionString
* EntityList

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
experiment with your API using a tool like GraphiQL, but not to allow Introspection in Production.

### ConnectionString

```csharp
public string ConnectionString { get; set; }
```

The connection string to your SQL Server or Azure SQL database.

### EntityList

```csharp
public List<EntityBase> EntityList { get; set; }
```

The list of entities that are mapped to your database.

Each entity in the list is an *Instance* of an entity. The typical pattern
is described in
[Create Entity List]({{ 'gettingStarted?topic=create-entity-list' | relative_url }})

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

When querying a list of items, `GraphqlToTsql` needs a plural form of the
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

Note: sometimes you might want an entity that maps
to a SQL query, not to a physical table. You are still required
a `DbTableName`, but you can choose up any name you want.
See: [Advanced Mappings]({{ 'documentation?topic=advanced-mappings' | relative_url }})

### EntityType (Optional)

The `GraphQL` Type name for the entity. This is the name 
you will use in [Fragments](https://graphql.org/learn/queries/#fragments).
It is also the name you will see if you're doing Introspection queries.

If you choose not to set `EntityType`, it defaults to the same as the `DbTableName`.

```csharp
public override string EntityType => "ButterflyType";
```

```graphql
# This GraphQL query shows how to query using a GraphQL fragment.
# The ... is the GraphQL syntax for "use fragment".
# Notice that the fragment is strongly typed.

{
  b1: butterfly (id: "Monarch") { ... butterflyFrag }
  b2: butterfly (id: "Black Swallowtail") { ... butterflyFrag }
}

fragment butterflyFrag on ButterflyType { genus species }
```

### PrimaryKeyFieldNames

The names of the Primary Key fields. Use the GraphQL names, not the SQL column names.

You must provide a non-empty array of field names. It is used for
`GraphQL` queries that use paging.

```csharp
public override string[] PrimaryKeyFieldNames => new[] { "butterflyId" };
```

See: [??? Paging]({{ 'documentation?topic=???' | relative_url }})

### SqlDefinition (Optional)

You'll probably use `SqlDefinition` only a handful of times. It's used to map
an entity to a SQL SELECT statement rather than a table.

For detailed instructions, see: 
[??? Virtual Table]({{ 'documentation?topic=???' | relative_url }})

### MaxPageSize (Optional)

`GraphqlToTsql` supports, but normally doesn't require, paged queries.
By setting `MaxPageSize` on an entity, you force queries to use paging
for the entity.

For example, if you want to limit the number of `Customer` rows that can be returned
in a single query to 100:

```csharp
public override long? MaxPageSize => 100L;
```

The `GraphQL` will then be required to use paging for all `Customer` lists:

```graphql
# These queries use "offset paging" to receive only 100 rows at a time
{ 
  customers (offset: 900, first: 100) { name } 
  regions { name customers (first: 100) { name } }
}
```

`GraphqlToSql` also supports cursor-based paging. Details are shown in the
Paging section.

See: [??? Paging]({{ 'documentation?topic=???' | relative_url }})

### BuildFieldList()

You must implement the `BuildFieldList` method to define all the
fields in your entity.

Each field is defined by calling a static factory method on
the `Field` class.

TODO

</div>

<div markdown="1">

# Advanced Mappings


```graphql

```

</div>






<div markdown="1">

# Mapping a TVF

`Field.CalculatedSet` can be used to map a database Table-Valued Function (TVF).

In the reference database there is a `Seller` table
* Keyed by `Name`
* With a `DistributorName` column that self-references the `Seller` table

The database has a TVF to find all the descendants for a distributor.

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
# Foo

</div>
