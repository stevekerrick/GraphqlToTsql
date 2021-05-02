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

## Settings

The `GraphqlActions` class is the top-level `GraphqlToTsql` class.
It has two public methods you can use to process a GraphQL query:
* `TranslateAndRunQuery`
* `TranslateToTsql`

Both methods have a required parameter of type
`GraphqlActionSettings` which has three properties.

```csharp
public class GraphqlActionSettings
{
    public bool AllowIntrospection { get; set; }
    public string ConnectionString { get; set; }
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
See: [Field Mapping]({{ 'documentation?topic=field-mapping' | relative_url }})

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

You'll probably use `SqlDefinition` only a handful of times. It's used to map
an entity to a SQL SELECT statement rather than to a table.

For detailed instructions, see: 
[Field Mapping]({{ 'documentation?topic=field-mapping' | relative_url }})

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

        Field.Set(OrderDetailEntity.Instance, "orderDetails", IsNullable.No, new Join(
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

## Mapping a Column

The most common mapping is Column Mapping. It defines an entity field that maps
to a database column.

To create a Column Mapping, use the static method `Field.Column()`.

```csharp
/// <summary>
/// Builds a field that maps to a database column. This is the factory method you'll use most often.
/// </summary>
/// <param name="entity">The entity this field belongs to</param>
/// <param name="name">The name of the field in the GraphQL</param>
/// <param name="dbColumnName">The column name in the database</param>
/// <param name="valueType">Data type of the column. One of: String, Int, Float, Boolean.</param>
/// <param name="isNullable">Is the database column nullable?</param>
/// <param name="visibility">Mark the field as "Hidden" if you don't want to expose it to GraphQL
/// queries. This is useful to hide Primary Key fields that are needed for joins.</param>
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
TODO: Show samples
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



### IsNullable isNullable (Required)


### Visibility visibitlity (Optional)



## Mapping a Related Row

## Mapping a Related Set

## Mapping a Calculated Value

## Mapping a Calculated Row

## Mapping a Calculated Set




## Mapping a TVF

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

# Paging

TODO

</div>








<div markdown="1">

# Foo

</div>
