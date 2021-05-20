---
layout: topicsPage
title: Demo
---

<div markdown="1">

# Demo Pages

<a href="//graphqltotsqldemoapp.azurewebsites.net/" target="_blank">Sample Queries</a>

There are two web pages that demonstrate the capabilities of `GraphqlToTsql`.

* [Sample Queries]({{ 'demo?topic=sample-queries' | relative_url }}) - A UI showing
a wide assortment of sample queries. The user can modify them
or create one of their own. After a query is run, the page shows the
resulting data, the TSQL that was generated, and some runtime statistics.

* [GraphiQL]({{ 'demo?topic=graphiql' | relative_url }}) - An open source in-browser IDE
for composing and testing GraphQL queries.

Important note: The demo website and database are hosted in Azure using
the least expensive options possible. (The App Service is F1 (Free) and the database is Basic).
Expect uneven performance.

</div>

<div markdown="1">

# Database Schema

The demo database has six tables. To keep things interesting, some of the tables
have auto-incrementing ID's, some have natural keys, and some have compound keys.

The `Seller` table is self-referencing.

The [Init Script](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/DemoEntities/DatabaseCreateScript.sql) has all the details, including the data that's scripted in.

![](images/schemaDiagram.png)
</div>

<div markdown="1">

# Foo

Sample Queries|//graphqltotsqldemoapp.azurewebsites.net/

GraphiQL|//graphqltotsqldemoapp.azurewebsites.net/graphiql

</div>
