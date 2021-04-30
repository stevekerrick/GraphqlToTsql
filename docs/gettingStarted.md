---
layout: topicsPage
title: Getting Started
---

<div markdown="1">

# Get GraphqlToTsql

`GraphqlToTsql` is a component that you can use in your existing application
to support read-only GraphQL queries. To get started, the first step is to
download the component. There are a couple ways of doing that.

## Method 1: NuGet

Use Visual Studio's `Manage NuGet Packages for Solution` GUI to add GraphqlToTsql
to one of your projects.

> OR

If you are using .Net Framework, use the `nuget.exe` CLI to download the package:

```shell
nuget install GraphqlToTsql -OutputDirectory packages
```

> OR

If you are using .Net Core, use the `dotnet.exe` CLI to download the package:

```shell
dotnet add package GraphqlToTsql
```

## Method 2: Clone the GitHub Repository
Clone the [repo](https://github.com/stevekerrick/GraphqlToTsql),
and copy project `GraphqlToTsql` into your solution. It targets `.NET Standard 2.0`
and has a small number of Nuget dependencies.
</div>

<div markdown="1">

# Create Entity Mapping

`GraphqlToTsql` uses the "Entity Mapping" pattern to define the types and fields
that will be accessible in the GraphQL, and to map them to tables and columns in your
database.

Getting started with `GraphqlToTsql` is not too hard. Most of your time
will be spent creating your entity mappings. `GraphqlToSql`
is flexible, allowing calculated fields, custom join criteria, virtual tables, and more.
The [Documentation page](/documentation) has guidance on all the
ways you can write the mapping.

For a simple example, suppose you have a table named `Product`, which is related to the `OrderDetail` table
by a foreign key column named `ProductName`.

![](images/productSchema.png)

The Entity for the `Product` table could look like this:

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

* See: The [Documentation page](/documentation)
* See: [Demo Entities](https://github.com/stevekerrick/GraphqlToTsql/tree/main/src/DemoEntities)
in the project repository for more examples.

</div>

<div markdown="1">

# Create Entity List

When calling on `GraphqlToTsql` to process a GraphQL query, you must pass in a `List<EntityBase>`,
with instances of all your entities.

The [Reference Application](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/DemoEntities/DemoEntityList.cs) uses a static class named `DemoEntityList` for this purpose:

```csharp
public static class DemoEntityList
{
    public static List<EntityBase> All()
    {
        return new List<EntityBase>
        {
            BadgeEntity.Instance,
            OrderEntity.Instance,
            OrderDetailEntity.Instance,
            ProductEntity.Instance,
            SellerEntity.Instance,
            SellerBadgeEntity.Instance,
            SellerProductTotalEntity.Instance,
            SellerTotalEntity.Instance
        };
    }
}
```

</div>

<div markdown="1">

# Register GraphqlActions

If your application uses a DI container (such as AspNetCore's [IServiceCollection](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection?view=aspnetcore-5.0)) there is one class to register.
* `GraphqlActions` implements interface `IGraphqlActions`.

For example:

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

# Wire Up the API

`GraphqlToTsql` is a component to translate a *GraphQL* query into a *TSQL* query, 
and to execute the query for you.
*You* provide the web service/controller, however.

The request/response models are therefore under your control.
But if you want your GraphQL endpoint to "play nice" with other components in the
GraphQL ecosphere, such as the [ReactJs Apollo Client](https://www.apollographql.com/docs/react/),
you need to follow the [GraphQL spec](https://graphql.org/learn/serving-over-http/),
which dictates the shapes of the request/response objects.

The `GraphqlToTsql` [Reference Application](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/DemoEntities/DemoEntityList.cs)
has an AspNetCore project that follows the spec.
Here is a slightly simplified version of the controller that exposes the endpoint `/api/graphql`.

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
                Errors = errors
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

    // The shape of the QueryResponse is dictated by the GraphQL standard
    public class QueryResponse
    {
        public object Data { get; set; }
        public string[] Errors { get; set; }
    }
}
```

</div>

<div markdown="1">

# Two Ways to Execute the Query

`GraphqlToTsql` is meant to be a flexible component of your .NET API.
It is happy to execute the GraphQL query -- simply supply the database connection string, 
and call `TranslateAndRunQuery`.

```csharp
var settings = new GraphqlActionSettings
{
    AllowIntrospection = false,
    EntityList = DemoEntityList.All(),
    ConnectionString = _configuration.GetConnectionString("DemoDB")
};

var queryResult = await _graphqlActions.TranslateAndRunQuery(graphql, graphqlParameters, settings);
```

If you prefer to have more control over the database access you can use `TranslateToTsql` to create the TSQL
query (and associated TSQL Parameters), then submit the query to the database yourself.

```csharp
var settings = new GraphqlActionSettings
{
    AllowIntrospection = false,
    EntityList = DemoEntityList.All()
};

var tsqlResult = await _graphqlActions.TranslateToTsql(graphql, graphqlParameters, settings);
// If tsqlResult.Error is not null it means the query was faulty
// Otherwise, the results are in tsqlResult.Tsql and tsqlResult.TsqlParameters
```

* See: [IGraphqlActions interface](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/GraphqlToTsql/GraphqlActions.cs)
* See: [DbAccess](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/GraphqlToTsql/Database/DbAccess.cs),
the class in `GraphqlToTsql` that sends queries to the database. It uses the Micro ORM [Dapper](https://github.com/StackExchange/Dapper)

</div>
