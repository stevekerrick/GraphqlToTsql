---
layout: topicsPage
title: Documentation
---

<div markdown="1">

# How to Use GraphqlToTsql

GraphqlToTsql is a component that translates a GraphQL query into a
monolithic TSQL query, and (optionally) sends it to a SQL Server or
AzureSQL database.

The main setup steps are covered on the Getting Started page:
* [Get GraphqlToTsql]({{ 'gettingStarted?topic=get_graphiqltotsql' | relative_url }})

Create Entity Mapping
Create Entity List
Register GraphqlActions
Wire Up the API
Optional: Wire Up the DB



Hmm, somehow this Controller will be useful...

```csharp
using DemoEntities;
using GraphqlToTsql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphqlController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IGraphqlActions _graphqlActions;

        public GraphqlController(
            IConfiguration configuration,
            IGraphqlActions graphqlActions)
        {
            _configuration = configuration; ;
            _graphqlActions = graphqlActions;
        }

        [HttpPost]
        public async Task<JsonResult> Post([FromBody] QueryRequest query)
        {
            var result = await RunQuery(query);

            var response = new { result.Data, result.Errors };
            return new JsonResult(response);
        }

        private async Task<QueryResponse> RunQuery(QueryRequest query)
        {
            var graphql = query.Query;
            var graphqlParameters = string.IsNullOrEmpty(query.Variables)
                ? null
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(query.Variables);

            var settings = new GraphqlActionSettings
            {
                AllowIntrospection = false,
                EntityList = DemoEntityList.All(),
                ConnectionString = _configuration.GetConnectionString("DemoDB")
            };

            var queryResult = await _graphqlActions.TranslateAndRunQuery(graphql, graphqlParameters, settings);

            var errors =
                queryResult.TranslationError != null ? new[] { queryResult.TranslationError }
                : queryResult.DbError != null ? new[] { queryResult.DbError }
                : null;

            return new QueryResponse
            {
                Data = Deserialize(queryResult.DataJson),
                Errors = errors,
                Tsql = queryResult.Tsql,
                TsqlParameters = queryResult.TsqlParameters,
                Statistics = queryResult.Statistics,
                ErrorCode = queryResult.ErrorCode.ToString()
            };
        }

        private static object Deserialize(string json)
        {
            if (json == null) return null;
            return JsonConvert.DeserializeObject(json);
        }
    }

    // The shape of the QueryRequest is dictated by the GraphQL standard
    public class QueryRequest
    {
        // The GraphQL query
        public string Query { get; set; }

        // The GraphQL variable values, in JSON format
        public string Variables { get; set; }
    }

    public class QueryResponse
    {
        // These parts are dictated by the GraphQL standard
        public object Data { get; set; }
        public string[] Errors { get; set; }

        // These parts are extra
        public string Tsql { get; set; }
        public Dictionary<string, object> TsqlParameters { get; set; }
        public List<Statistic> Statistics { get; set; }
        public string ErrorCode { get; set; }
    }
}
```

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
