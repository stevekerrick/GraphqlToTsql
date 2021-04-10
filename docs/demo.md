---
layout: topicsPage
title: Demo
---

<div markdown="1">

# Demo Pages

There are two web pages that demonstrate the capabilities of `GraphqlToTsql`.

* [Sample Queries]({{ 'demo?topic=sample-queries' | relative_url }}) - A UI showing
    a wide assortment of sample queries. The user can modify them
    or create one of their own. After a query is run, the page shows the
    resulting data, the TSQL that was generated, and some runtime statistics.

* [GraphiQL]({{ 'demo?topic=graphiql' | relative_url }}) - A graphical interactive in-browser GraphQL IDE

(Note: [GraphiQL](https://github.com/graphql/graphiql/blob/main/packages/graphiql/README.md)
is an open-source IDE for composing and testing GraphQL queries.)

</div>

<div markdown="1">

# Database Schema

The demo database has six tables. To keep things interesting, some of the tables
have auto-incrementing ID's, some have natural keys, and two have compound keys.

The `Seller` table is self-referencing.

The [Init Script](https://github.com/stevekerrick/GraphqlToTsql/blob/main/src/DemoEntities/DatabaseCreateScript.sql) has all the details, including the data that's scripted in.

![](images/schemaDiagram.png)
</div>

<div markdown="1">

# Sample Queries|//graphqltotsqldemoapp.azurewebsites.net/

</div>

<div markdown="1">

# GraphiQL|//graphqltotsqldemoapp.azurewebsites.net/graphiql

</div>
