﻿using DemoEntities;
using GraphqlToTsql.Translator;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class InvalidRequestTests : IntegrationTestBase
    {
        [Test]
        public void FaultyGraphqlTest()
        {
            var graphql = "my faulty graphql";
            ParseShouldFail(graphql, null, "mismatched input 'my'");
        }

        [Test]
        public void EmptySelectionSetTest()
        {
            var graphql = "{ products { } }";
            ParseShouldFail(graphql, null, "mismatched input '}'");
        }

        [Test]
        public void UnsupportedInlineFragmentTest()
        {
            var graphql = @"
{
  orders {
    date
    ... on Seller {
      name
    }
  }
}
".Trim();

            ParseShouldFail(graphql, null, "Inline Fragments");
        }

        [Test]
        public void UnsupportedDirectiveTest()
        {
            var graphql = @"
{
  orders {
    seller @include(if: true) {
      name
    }
  }
}
".Trim();

            ParseShouldFail(graphql, null, "Directives");
        }

        [Test]
        public void FirstArgumentMustBeIntTest()
        {
            const string graphql = "{ products { orderDetailsConnection (first: \"oops\") { totalCount } } }";

            ParseShouldFail(graphql, null, "first must be an integer");
        }

        [Test]
        public void OffsetArgumentMustBeIntTest()
        {
            const string graphql = "{ products { orderDetailsConnection (offset: \"oops\") { totalCount } } }";

            ParseShouldFail(graphql, null, "offset must be an integer");
        }

        [Test]
        public void AfterArgumentMustBeStringTest()
        {
            const string graphql = "{ products { orderDetailsConnection (after: 66) { totalCount } } }";

            ParseShouldFail(graphql, null, "after must be a string");
        }

        [Test]
        public void UsingBothOffsetAndAfterTest()
        {
            const string graphql = "{ products { orderDetailsConnection (first: 2, offset: 3, after: \"myCursor\") { totalCount } } }";

            ParseShouldFail(graphql, null, "You can't use 'offset' and 'after' at the same time");
        }

        [Test]
        public void DeclaredVariableWithoutValueTest()
        {
            const string graphql = "query mytest($id: Int) { order (id: $id) { shipping } }";

            ParseShouldFail(graphql, null, "Variable [$id] is used in the query, but doesn't have a value");
        }

        [Test]
        public void FragmentWithUnknownTypeTest()
        {
            var graphql = @"
{ order { ... frag} }
fragment frag on Arggh { id }
".Trim();

            ParseShouldFail(graphql, null, "Unknown type: Arggh");
        }

        [Test]
        public void UnknownEntityTest()
        {
            const string graphql = "{ Arggh { id } }";

            ParseShouldFail(graphql, null, "Unknown entity: Arggh");
        }

        [Test]
        public void UnknownFieldTest()
        {
            const string graphql = "{ order { id arggh shipping } }";

            ParseShouldFail(graphql, null, "Unknown field: order.arggh");
        }

        [Test]
        public void UndeclaredVariableTest()
        {
            const string graphql = "{ order(id: $Arggh) { shipping } }";

            ParseShouldFail(graphql, null, "Variable [$Arggh] is not declared");
        }

        [Test]
        public void ArgumentOnEdgeTest()
        {
            const string graphql = "{ sellers { ordersConnection { edges(cursor: \"hi\") { cursor } } } }";

            ParseShouldFail(graphql, null, "Arguments are not allowed on [edges]");
        }

        [Test]
        public void ArgumentOnNodeTest()
        {
            const string graphql = "{ sellers { ordersConnection { edges { node(id: 1) { id } } } } }";

            ParseShouldFail(graphql, null, "Arguments are not allowed on [node]");
        }

        [Test]
        public void ArgumentWithoutNameTest()
        {
            // Funny, but our GQL language allows this malformed query, so we throw our own exception
            const string graphql = "{ orders (id = 1) { id } }";

            ParseShouldFail(graphql, null, "Arguments should be formed like (id: 1)");
        }

        [Test]
        public void ArgumentOnFieldTest()
        {
            const string graphql = "{ seller { name (showAs: \"hex\") } }";

            ParseShouldFail(graphql, null, "Arguments are not allowed on [name]");
        }

        // The QueryBuilder is also exercised in the Parse step, and that's really where most of the errors are being found
        private void ParseShouldFail(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            string partialErrorMessage)
        {
            var parser = GetService<IParser>();
            var parseResult = parser.ParseGraphql(graphql, graphqlParameters, DemoEntityList.All());
            Console.WriteLine(parseResult.ParseError);

            Assert.IsNotNull(parseResult.ParseError, "Expected parse to fail, but it succeeded");
            Assert.IsTrue(parseResult.ParseError.Contains(partialErrorMessage),
                $"Unexpected error message. Expected [{partialErrorMessage}] but found [{parseResult.ParseError}]");
        }
    }
}
