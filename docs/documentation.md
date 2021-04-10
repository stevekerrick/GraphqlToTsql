---
layout: topicsPage
title: Documentation
---

<div markdown="1">
# Mapping a Table Valued Function

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
