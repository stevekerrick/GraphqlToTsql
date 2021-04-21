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
and both methods require an instance of `GraphqlActionSettings`. `GraphqlActionSettings`
has three properties:

### AllowIntrospection

```csharp
public bool AllowIntrospection { get; set; }
```

An important part of `GraphQL` is [Introspection](https://graphql.org/learn/introspection/).
Introspection is a way of making `GraphQL` queries to discover the kinds of data
that is available. For example, this Introspection query finds all the Types (a.k.a. entities)
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

### EntityList

```csharp
public List<EntityBase> EntityList { get; set; }
```



## Entity Mappings

The remainder of this Documentation page explores the different ways to map entities
to your database.








</div>

<div markdown="1">

# Options/settings




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
