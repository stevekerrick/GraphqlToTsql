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
        public void FirstArgumentMustBeIntTest()
        {
            const string graphql = "{ products { orderDetailsConnection (first: \"oops\") { totalCount } } }";

            ParseShouldFail(graphql, null, "first must be an Int");
        }

        [Test]
        public void OffsetArgumentMustBeIntTest()
        {
            const string graphql = "{ products { orderDetailsConnection (offset: \"oops\") { totalCount } } }";

            ParseShouldFail(graphql, null, "offset must be an Int");
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

            ParseShouldFail(graphql, null, "Variable $id is used in the query, but doesn't have a value");
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

            ParseShouldFail(graphql, null, "Unknown field: Order.arggh");
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

        [Test]
        public void MissingFirstOnMaxPageSizeTest()
        {
            const string graphql = "{ orders { id } }";

            TsqlGenerationShouldFail(graphql, null, "Paging is required with orders");
        }

        [Test]
        public void FirstIsOverMaxPageSizeTest()
        {
            const string graphql = "{ orders (first: 2000) { id } }";

            TsqlGenerationShouldFail(graphql, null, "The max page size for orders is 1000");
        }

        [Test]
        public void FragmentIsNotDefinedTest()
        {
            const string graphql = "{ sellers { ... sellerFields } }";

            TsqlGenerationShouldFail(graphql, null, "Fragment is not defined: sellerFields");
        }

        [Test]
        public void FragmentIsForWrongTypeTest()
        {
            var graphql = @"
{ badges { ... frag} }
fragment frag on Seller { name }
".Trim();

            TsqlGenerationShouldFail(graphql, null, "Fragment frag is defined for Seller, not Badge");
        }

        [Test]
        public void ArgumentTypeFieldTypeMismatchTest()
        {
            const string graphql = "{ seller (name: 1234) { distributorName } }";

            ParseShouldFail(graphql, null, "Argument is the wrong type: Seller.name is type String");
        }

        [Test]
        public void VariableTypeParameterTypeMismatchTest()
        {
            const string graphql = "query test($myId: Int) { order (id: $myId) { sellerName } }";
            var graphqlParameters = new Dictionary<string, object> { { "myId", "1234" } };

            ParseShouldFail(graphql, graphqlParameters, "Variable value is the wrong type: $myId is type Int");
        }

        [Test]
        public void NullValueOnNonNullableFieldTest()
        {
            const string graphql = "query test($myId: Int) { order (id: $myId) { date } }";
            var graphqlParameters = new Dictionary<string, object> { { "myId", null } };

            ParseShouldFail(graphql, graphqlParameters, "Order.id can not be null");
        }

        [Test]
        public void NullValueForNonNullableVariableTest()
        {
            const string graphql = "query test($name: String!) { seller (sellerName: $name) { name } }";
            var graphqlParameters = new Dictionary<string, object> { { "name", null } };

            ParseShouldFail(graphql, graphqlParameters, "Invalid null value: Variable $name is not nullable");
        }

        [Test]
        public void HiddenFieldTest()
        {
            const string graphql = "{ sellerBadges { sellerName } }";

            ParseShouldFail(graphql, null, "Unknown field: sellerBadge.sellerName");
        }
    }
}
