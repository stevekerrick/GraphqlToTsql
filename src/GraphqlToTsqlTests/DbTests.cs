using DemoEntities;
using GraphqlToTsql;
using GraphqlToTsql.Entities;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class DbTests : IntegrationTestBase
    {
        [Test]
        public async Task SimpleQueryTest()
        {
            const string graphql = "{ orders (first: 10, id: 1) { id } }";
            var graphqlParameters = new Dictionary<string, object> { };

            var expectedObject = new { orders = new[] { new { id = 1 } } };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task BoolValueQueryTest()
        {
            const string graphql = "{ badges (isSpecial: true) { name isSpecial } }";
            var graphqlParameters = new Dictionary<string, object> { { "isSpecial", true } };

            var expectedObject = new
            {
                badges = new[]
                {
                    new { name = "Diamond", isSpecial = true },
                    new { name = "Founder", isSpecial = true }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task DateQueryTest()
        {
            var graphql = @"
{
  orders (first: 10, date: ""2020-01-29"") {
    id date seller { name sellerBadges(first: 1) { badge { name } dateAwarded } }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object>();

            var expectedObject = new
            {
                orders = new[]
                {
                    new {
                        id = 2,
                        date = "2020-01-29",
                        seller = new {
                            name = "Bill",
                            sellerBadges = new[] {
                                new { badge = new { name = "Diamond" }, dateAwarded = "2020-04-07" }
                            }
                        }
                    }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task NonNullableListTest()
        {
            const string graphql = "{ seller (name: \"Zeus\") { name sellerBadges { badge { name } } } }";
            var graphqlParameters = new Dictionary<string, object>();

            var expectedObject = new
            {
                seller = new {
                    name = "Zeus",
                    sellerBadges = new object[0]
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject, EmptySetBehavior.EmptyArray);
        }

        [Test]
        public async Task IntrospectionTypeNameTest()
        {
            var graphql = @"
{
  __type (name: ""Badge"") {
    kind name fields { name }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object>();

            var expectedObject = new
            {
                __type = new
                {
                    kind = "OBJECT",
                    name = "Badge",
                    fields = new[]
                    {
                        new { name = "name" },
                        new { name = "isSpecial" },
                        new { name = "sellerBadges" }
                    }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task IntrospectionSchemaTest()
        {
            var graphql = @"
{
  __schema {
    types (name: ""Order"") { name }
    queryType { name }
    mutationType { name }
    subscriptionType { name }
    directives { name }
  }
}".Trim();

            var graphqlParameters = new Dictionary<string, object>();

            var expectedObject = new
            {
                __schema = new
                {
                    types = new[] { new { name = "Order" } },
                    queryType = new { name = "Query" },
                    mutationType = (object)null, //not supported
                    subscriptionType = (object)null, //not supported
                    directives = new[] { new { name = "include" }, new { name = "skip" } }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task IntrospectionOfTypeTest()
        {
            // This is a confusing query, but it's a typical Introspection query.
            // (Except that filtering on the fields in not supported in vanilla GraphQL.)
            // For type __Type, for field named "fields", show the type buildup for it.
            // __Type.fields is type LIST (NON_NULL (OBJECT) )
            var graphql = @"
{
  __type (name: ""__Type"") {
    fields (name: ""fields"") {
      name type { kind name ofType { kind name ofType { kind name ofType { kind name ofType { name } } } } }
    }
  }
}".Trim();

            var graphqlParameters = new Dictionary<string, object>();

            var expectedObject = new
            {
                __type = new
                {
                    fields = new[]
                    {
                        new {
                            name = "fields",
                            type = new
                            {
                                kind = "LIST",
                                name = (string)null,
                                ofType = new {
                                    kind = "NON_NULL",
                                    name = (string)null,
                                    ofType = new {
                                        kind = "OBJECT",
                                        name = "__Field",
                                        ofType = (object)null
                                    }
                                }
                            }
                        }
                    }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task IntrospectionEnumValueTest()
        {
            var graphql = @"
{
  __type (name: ""__TypeKind"") {
    enumValues { name }
  }
}".Trim();

            var graphqlParameters = new Dictionary<string, object>();

            var expectedObject = new
            {
                __type = new
                {
                    enumValues = new[]
                    {
                        new { name = "SCALAR" },
                        new { name = "OBJECT" },
                        new { name = "INTERFACE" },
                        new { name = "UNION" },
                        new { name = "ENUM" },
                        new { name = "INPUT_OBJECT" },
                        new { name = "LIST" },
                        new { name = "NON_NULL" }
                    }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task IntrospectionInputValueTest()
        {
            var graphql = @"
{
  __type (name: ""SellerBadge"") {
    fields (name: ""badge"") {
      args { name type { name } }
    }
  }
}".Trim();

            var graphqlParameters = new Dictionary<string, object>();

            var expectedObject = new
            {
                __type = new
                {
                    fields = new[]
                    {
                        new
                        {
                            args = new[]
                            {
                                new { name = "name", type = new { name = "String" } },
                                new { name = "isSpecial", type = new { name = "Boolean" } }
                            }
                        }
                    }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task IntrospectionHiddenFieldTest()
        {
            var graphql = @"
{
  __type (name: ""SellerBadge"") {
    kind name fields { name }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object>();

            // Notice that SellerName and BadgeName are hidden
            var expectedObject = new
            {
                __type = new
                {
                    kind = "OBJECT",
                    name = "SellerBadge",
                    fields = new[]
                    {
                        new { name = "dateAwarded" },
                        new { name = "seller" },
                        new { name = "badge" }
                    }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task GraphiqlIntrospectionTest()
        {
            // This is the introspection query that Graphiql sends upon start
            var graphql = @"
   query IntrospectionQuery {
      __schema {
        
        queryType { name }
        mutationType { name }
        subscriptionType { name }
        types {
          ...FullType
        }
        directives {
          name
          description
          
          locations
          args {
            ...InputValue
          }
        }
      }
    }

    fragment FullType on __Type {
      kind
      name
      description
      
      fields(includeDeprecated: true) {
        name
        description
        args {
          ...InputValue
        }
        type {
          ...TypeRef
        }
        isDeprecated
        deprecationReason
      }
      inputFields {
        ...InputValue
      }
      interfaces {
        ...TypeRef
      }
      enumValues(includeDeprecated: true) {
        name
        description
        isDeprecated
        deprecationReason
      }
      possibleTypes {
        ...TypeRef
      }
    }

    fragment InputValue on __InputValue {
      name
      description
      type { ...TypeRef }
      defaultValue
    }

    fragment TypeRef on __Type {
      kind
      name
      ofType {
        kind
        name
        ofType {
          kind
          name
          ofType {
            kind
            name
            ofType {
              kind
              name
              ofType {
                kind
                name
                ofType {
                  kind
                  name
                  ofType {
                    kind
                    name
                  }
                }
              }
            }
          }
        }
      }
    }

".Trim();

            var graphqlParameters = new Dictionary<string, object>();

            await RunAsync(graphql, graphqlParameters, EmptySetBehavior.Null);
        }

        private async Task CheckAsync(string graphql, Dictionary<string, object> graphqlParameters, object expectedObject,
            EmptySetBehavior emptySetBehavior = EmptySetBehavior.Null)
        {
            var queryResult = await RunAsync(graphql, graphqlParameters, emptySetBehavior);

            var dataObj = JsonConvert.DeserializeObject(queryResult.DataJson);
            var dataFormattedJson = JsonConvert.SerializeObject(dataObj, Formatting.Indented);

            var expectedFormattedJson = JsonConvert.SerializeObject(expectedObject, Formatting.Indented);
            Assert.AreEqual(expectedFormattedJson, dataFormattedJson, "Database response does not match expected");
        }

        private async Task<QueryResult> RunAsync(string graphql, Dictionary<string, object> graphqlParameters, EmptySetBehavior emptySetBehavior)
        {
            var graphqlActions = GetService<IGraphqlActions>();

            var settings = new GraphqlActionSettings
            {
                AllowIntrospection = true,
                ConnectionString = GetConnectionString(),
                EmptySetBehavior = emptySetBehavior,
                EntityList = DemoEntityList.All()
            };
            var queryResult = await graphqlActions.TranslateAndRunQuery(graphql, graphqlParameters, settings);

            Assert.IsNull(queryResult.TranslationError, $"The parse failed: {queryResult.TranslationError}");
            Console.WriteLine(queryResult.Tsql);
            Console.WriteLine(JsonConvert.SerializeObject(queryResult.TsqlParameters, Formatting.Indented));

            Assert.IsNull(queryResult.DbError, $"The database query failed: {queryResult.DbError}");

            var dataObj = JsonConvert.DeserializeObject(queryResult.DataJson);
            var dataFormattedJson = JsonConvert.SerializeObject(dataObj, Formatting.Indented);
            Console.WriteLine("");
            Console.WriteLine(dataFormattedJson);

            return queryResult;
        }
    }
}
