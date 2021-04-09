---
layout: topicsPage
title: Getting Started
---

<div markdown="1">

# Get GraphqlToTsql

Two easy ways to get `GraphqlToTsql`.

## Get the NuGet Package

### Option 1
Use Visual Studio's `Manage NuGet Packages for Solution` GUI to add GraphqlToTsql
to one of your projects.

### Option 2
If you are using .Net Framework, use the `nuget.exe` CLI to download the package:

```shell
nuget install Newtonsoft.Json -OutputDirectory packages
```

### Option 3
If you are using .Net Core, use the `dotnet.exe` CLI to download the package:

```shell
dotnet add package GraphqlToTsql
```

## Or Download the Code
Clone the [repo](https://github.com/stevekerrick/GraphqlToTsql),
and include project `GraphqlToTsql` in your solution.
</div>

<div markdown="1">

# Create Entity Mapping

`GraphqlToTsql` uses a pattern called "Entity Mapping" to define the types and fields
that will be accessible in the GraphQL, and to map them to tables and columns in the
database.

For example, suppose you have a table named `Product`, which is related to the `OrderDetail` table
by a foreign key column named `ProductName`.

![](images/productSchema.png)

You could map the `Product` table to a GraphQL entity named `Product` using an Entity Mapping like this:

```csharp
public class ProductDef : EntityBase
{
    public static ProductDef Instance = new ProductDef();

    public override string Name => "product";
    public override string DbTableName => "Product";
    public override string[] PrimaryKeyFieldNames => new[] { "name" };

    protected override List<Field> BuildFieldList()
    {
        return new List<Field>
        {
            Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
            Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),
            Field.Column(this, "price", "Price", ValueType.Float, IsNullable.No),

            Field.Set(OrderDetailDef.Instance, "orderDetails", IsNullable.Yes, new Join(
                ()=>this.GetField("name"),
                ()=>OrderDetailDef.Instance.GetField("productName"))
            )
        };
    }
}
```

* There is a lot of flexibility in the mapping: calculated fields, custom join criteria,
using Table Valued Functions, virtual tables, and more. See: [GraphqlToTsql Documentation](/documentation)
* For more examples, there is a reference application. See: [Demo Entities](https://github.com/stevekerrick/GraphqlToTsql/tree/main/src/DemoEntities)

</div>

<div markdown="1">

# Register GraphqlActions

Most applications use a DI container (such as AspNetCore's [IServiceCollection](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection?view=aspnetcore-5.0)). If your application doesn't, you can skip this section.

The main class in `GraphqlToTsql` is named `GraphqlActions`, and it impelements interface `IGraphqlActions`.
You need to register that class.

For example, if you are using AspNetCore you could register `GraphqlActions` in the `ConfigureServices`
method of `Startup`.

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddScoped<IGraphqlActions, GraphqlActions>();
    }
}
```
</div>

<div markdown="1">

# Wire up the API

The goal of `GraphqlToTsql` is to translate a GraphQL query into a TSQL query, and to execute the query for you.
You provide the web service, write the controller, and do authentication/authorization.

One thing to keep in mind: if you want to use the GraphQL query language with any of the standard tools in the GraphQL ecosystem,
such as [Apollo Client](https://www.apollographql.com/docs/react/), your API endpoint will need to comply with the standard,
including how to report errors.

The `GraphqlToTsql` repo has a reference AspNetCore project that follows the spec. Here is the controller that exposes
the endpoint `/api/graphql`.

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
        public async Task<JsonResult> Post([FromBody] QueryRequest query, bool? showStatistics)
        {
            var result = await RunQuery(query);

            object response;
            if (showStatistics.GetValueOrDefault())
            {
                response = result;
            }
            else if (result.Errors != null)
            {
                response = new { result.Errors, ErrorCode = result.ErrorCode.ToString() };
            }
            else
            {
                response = new { result.Data };
            }

            return new JsonResult(response);
        }

        private async Task<QueryResponse> RunQuery(QueryRequest query)
        {
            var graphql = query.Query;
            var graphqlParameters = string.IsNullOrEmpty(query.Variables)
                ? null
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(query.Variables);

            var connectionString = _configuration.GetConnectionString("DemoDB");
            if (string.IsNullOrEmpty(connectionString) || connectionString == "set in azure")
            {
                throw new Exception("Database connection string is not set");
            }

            var settings = new GraphqlActionSettings
            {
                AllowIntrospection = true,
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

# Advanced: Use Custom Database Access

The goal of `GraphqlToTsql` is to translate a GraphQL query into a TSQL query. Optionally, `GraphqlToTsql` can also
send the TSQL to the database and fetch the resulting JSON. You just supply a connection string.

If you prefer to wire up your own database access, use `GraphqlActions.TranslateToTsql()` to get the
TSQL plus parameters, then use your favorite SQL Connection tool to submit the query.

* See: [IGraphqlActions interface](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/GraphqlToTsql/GraphqlActions.cs)
* See: The [DbAccess](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/GraphqlToTsql/Database/DbAccess.cs)
is how `GraphqlToTsql` access the database using [Dapper](https://github.com/StackExchange/Dapper)

</div>
