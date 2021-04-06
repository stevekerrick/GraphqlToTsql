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
that will be accessible in the GraphQL, and how those types and fields look in the
database.

For example, suppose you have a table named Product, which is related to the OrderDetail table
by a foreign key named ProductName.

![](images/productSchema.png)

You could map the Product table to a GraphQL entity named Product using an Entity Mapping like this:

```csharp
using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace DemoEntities
{
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
}
```

* See: [GraphqlToTsql Documentation](/documentation)

</div>

<div markdown="1">
# Register GraphqlActions
Register the main `GraphqlToTsql` component, which is named `GraphqlActions`, with your DI container.

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
# Wire up the Database
TODO


</div>

<div markdown="1">
# Wire up the API
TODO


</div>
