---
layout: topicsPage
title: Documentation
---

<div markdown="1">

# How to Use GraphqlToTsql

GraphqlToTsql is a component that translates a GraphQL query into a
monolithic TSQL query, and (optionally) sends it to a SQL Server or
AzureSQL database.

## Setup

The main setup steps are covered on the [Getting Started page]({{ 'gettingStarted' | relative_url }}):
* [Get GraphqlToTsql]({{ 'gettingStarted?topic=get-graphqltotsql' | relative_url }})
* [Create Entity Mapping]({{ 'gettingStarted?topic=create-entity-mapping' | relative_url }})
* [Create Entity List]({{ 'gettingStarted?topic=create-entity-list' | relative_url }})
* [Register GraphqlActions]({{ 'gettingStarted?topic=register-graphqlactions' | relative_url }})
* [Wire Up the API]({{ 'gettingStarted?topic=wire-up-the-api' | relative_url }})
* [Optional: Wire Up the DB]({{ 'gettingStarted?topic=optional-wire-up-the-db' | relative_url }})

## GraphqlActionSettings

`IGraphqlActions` has two methods, `TranslateAndRunQuery` and `TranslateToTsql`,
and both require an instance of `GraphqlActionSettings`. That Settings class
has three properties:

### AllowIntrospection

```csharp
public bool AllowIntrospection { get; set; }
```

An important part of `GraphQL` is [Introspection](https://graphql.org/learn/introspection/).
Introspection is a way of using `GraphQL` queries to discover the kinds of data
that is available. For example, the Introspection query below finds all the Types (a.k.a. entities)
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

`GraphqlToTsql` supports Introspection queries, but you might not want to allow them
* Introspection queries are kind of slow (adds an extra second or two)
* Best Practice is to allow Introspection in test environments but not in Production

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

## Entity Mappings

The remainder of this Documentation page explores the different ways to map entities
to your database.

</div>

<div markdown="1">

# Entity Mapping Basics

Each of your Entities will inherit from [EntityBase.cs](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/GraphqlToTsql/Entities/EntityBase.cs).

## EntityBase

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

### Name

The `Name` to use for this entity in the `GraphQL` queries. Must be singular,
and start with a lower-case character.
It is common to give your entity the same name as the underlying database table
(but lower-cased). For example, `butterfly`.

It would appear in a `GraphQL` query like this:
```graphql
query { butterfly (id: "Monarch") { genus species } }
```

### PluralName (Optional)

When querying a list of items, `GraphqlToTsql` needs a plural form of the
`Name`. By default `GraphqlToSql` appends an "s" to the `Name`. For `Name`s
where that doesn't work, you need to supply the `PluralName`.

For example:
```csharp
public override string Name => "butterfly";
public override string PluralName => "butterflies";
```

```graphql
query { butterflies { genus species } }
```

### DbTableName

The name of the database table this entity maps to.

Sometimes you will want to expose an entity in the `GraphQL` that maps
to a SQL query, not to a physical table. You still need to supply
a `DbTableName`, but you can make up any name you want.
See: [???]({{ 'documentation?topic=???' | relative_url }})

### EntityType (Optional)

The `GraphQL` Type name for the entity. This is the name that's returned
in Introspection queries, in error messages if a query on the type is
faulty, and in [Fragments](https://graphql.org/learn/queries/#fragments).

The `EntityType` defaults to be the same as the `DbTableName`, which is
nearly always what you want.

```csharp
public override string EntityType => "ButterflyType";
```

```graphql
{
  b1: butterfly (id: "Monarch") { ... butterflyFrag }
  b2: butterfly (id: "Black Swallowtail") { ... butterflyFrag }
}

fragment butterflyFrag on ButterflyType { genus species }
```

### PrimaryKeyFieldNames

The names of the Primary Key fields. Use the GraphQL names, not the SQL column names.

You must provide a non-empty array of field names. It is used when the
`GraphQL` query uses paging.

```csharp
public override string[] PrimaryKeyFieldNames => new[] { "butterflyId" };
```

See: [??? Paging]({{ 'documentation?topic=???' | relative_url }})

### SqlDefinition (Optional)

You'll probably use `SqlDefinition` only a handful of times. It's used when
your entity is not mapped to a database table, but to a SQL SELECT statement.

This documentation page has a topic with detailed instructions.

See: [??? Virtual Table]({{ 'documentation?topic=???' | relative_url }})

### MaxPageSize (Optional)

`GraphqlToTsql` supports paged queries, but normally doesn't require paging to be used.
You can require some lists of entities to be paged.

For example, if you want to limit the number of `Customer` rows that can be returned
in a single query to 100:

```csharp
public override long? MaxPageSize => 100L;
```

The `GraphQL` will then be required to use paging for all `Customer` lists:

```graphql
{ 
  customers (offset: 900, first: 100) { name } 
  regions { name customers (first: 100) { name } }
}
```

`GraphqlToSql` also supports cursor-based paging.

See: [??? Paging]({{ 'documentation?topic=???' | relative_url }})

### BuildFieldList()





    /// <summary>
    /// You must implement this method to populate the list of entity fields.
    /// This is the hardest part of your entity mapping. You'll use the static Field
    /// factory methods.
    /// </summary>


</div>

<div markdown="1">

# Mapping a Column


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
