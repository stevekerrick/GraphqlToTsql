using DemoEntities;
using GraphqlToTsql;
using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
  sellerBadges (dateAwarded: ""2020-09-16"") {
    dateAwarded
    seller { name }
    badge { name }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object>();

            var expectedObject = new
            {
                sellerBadges = new[]
                {
                    new
                    {
                        dateAwarded = "2020-09-16",
                        seller = new { name = "Willem" },
                        badge = new { name = "Bronze" }
                    }
                }
            };
            await CheckAsync(graphql, graphqlParameters, expectedObject);
        }

        [Test]
        public async Task DateTimeOffsetQueryTest()
        {
            var graphql = @"
query ($sellerName: String) {
  seller (name: $sellerName) {
    orders (first: 10) {
      date shipping
    }
  }
}".Trim();
            var graphqlParameters = new Dictionary<string, object> { { "sellerName", "Helena" } };

            var expectedObject = new
            {
                seller = new
                {
                    orders = new[]
                    {
                        new
                        {
                            date = "2020-08-17T20:15:02-05:00",
                            shipping = 21.95
                        },
                        new
                        {
                            date = "2020-09-10T19:00:00Z",
                            shipping = 9.95
                        },
                        new
                        {
                            date = "2020-09-16T19:15:02Z",
                            shipping = 7.95
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
        public async Task CursorTest()
        {
            // Query the first page
            var page1Graphql = @"
query firstPage ($seller: String) { 
  seller (name: $seller) {
    ordersConnection (first: 1) {
      totalCount
      edges {
        cursor
        node {
          id
        }
      }
    }
  } 
}".Trim();
            var page1Parameters = new Dictionary<string, object> 
            {
                { "seller", "bill" }
            };

            var page1Result = await RunAsync(page1Graphql, page1Parameters, EmptySetBehavior.Null);
            Assert.AreEqual(ErrorCode.NoError, page1Result.ErrorCode);

            // Pick out the Cursor
            var json = JObject.Parse(page1Result.DataJson);
            var totalCountJtoken = json.SelectToken("$.seller.ordersConnection.totalCount");
            Assert.AreEqual(6, ((JValue)totalCountJtoken).Value);

            var cursorJtoken = json.SelectToken("$.seller.ordersConnection.edges[0].cursor");
            var cursor = (string)((JValue)cursorJtoken).Value;

            // Use the cursor in the 2nd query
            var page2Graphql = @"
query firstPage ($seller: String, $cursor: String) { 
  seller (name: $seller) {
    ordersConnection (first: 1, after: $cursor) {
      totalCount
      edges {
        cursor
        node {
          id
        }
      }
    }
  } 
}".Trim();
            var page2Parameters = new Dictionary<string, object>
            {
                { "seller", "bill" },
                { "cursor", cursor }
            };

            var page2Result = await RunAsync(page2Graphql, page2Parameters, EmptySetBehavior.Null);
            Assert.AreEqual(ErrorCode.NoError, page2Result.ErrorCode);

            // Pick out the totalCount and OrderId
            json = JObject.Parse(page1Result.DataJson);

            totalCountJtoken = json.SelectToken("$.seller.ordersConnection.totalCount");
            Assert.AreEqual(6, ((JValue)totalCountJtoken).Value);

            var orderIdJtoken = json.SelectToken("$.seller.ordersConnection.edges[0].node.id");
            var orderId = (long)((JValue)orderIdJtoken).Value;
            Assert.AreEqual(2L, orderId);
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
