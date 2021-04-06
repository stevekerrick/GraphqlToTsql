---
layout: topicsPage
title: Getting Started
---

<div markdown="1">
# Download GraphqlToTsql
Get the code one of three ways.

## Get the NuGet Package

### Option 1
Use Visual Studio's `Manage NuGet Packages for Solution` GUI to add GraphqlToTsql
to one of your projects.

### Option 2
If your project uses the .Net Framework, you can use the Package Manager Console 
(or another command line) to install GraphqlToTsql:
`nuget install Newtonsoft.Json -OutputDirectory packages`

### Option 3
If your project uses .Net Core, `dotnet add package GraphqlToTsql`

## Or Download the Code
Clone the [repo](https://github.com/stevekerrick/GraphqlToTsql),
and include the `GraphqlToTsql` project in your solution.
</div>

<div markdown="1">
# Create Entity Mapping
TODO


</div>

<div markdown="1">
# Register GraphqlActions
Register the main `GraphqlToTsql` component, which is named `GraphqlActions`, with your DI container.

For example, if you are using AspNetCore you could register `GraphqlActions` in the `ConfigureServices`
method of `Startup`.

```C#
public class Startup
{
    ...

    public void ConfigureServices(IServiceCollection services)
    {
        ...

        services
            .AddScoped<IGraphqlActions, GraphqlActions>();
    }

    ...
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
