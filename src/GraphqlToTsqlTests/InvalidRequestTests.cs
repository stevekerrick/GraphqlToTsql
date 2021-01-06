using DemoEntities;
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
            var graphql = "{ epcs { } }";
            ParseShouldFail(graphql, null, "mismatched input '}'");
        }

        [Test]
        public void UnsupportedInlineFragmentTest()
        {
            var graphql = @"
{
  products {
    urn
    ... on Lot {
      lotNumber
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
  products {
    lots @include(if: true) {
      lotNumber
    }
  }
}
".Trim();

            ParseShouldFail(graphql, null, "Directives");
        }

        [Test]
        public void FirstArgumentMustBeIntTest()
        {
            const string graphql = "{ products { lotsConnection (first: \"oops\") { totalCount } } }";

            ParseShouldFail(graphql, null, "first must be an integer");
        }

        [Test]
        public void OffsetArgumentMustBeIntTest()
        {
            const string graphql = "{ products { lotsConnection (offset: \"oops\") { totalCount } } }";

            ParseShouldFail(graphql, null, "offset must be an integer");
        }

        [Test]
        public void AfterArgumentMustBeStringTest()
        {
            const string graphql = "{ products { lotsConnection (after: 66) { totalCount } } }";

            ParseShouldFail(graphql, null, "after must be a string");
        }

        [Test]
        public void UsingBothOffsetAndAfterTest()
        {
            const string graphql = "{ products { lotsConnection (first: 2, offset: 3, after: \"myCursor\") { totalCount } } }";

            ParseShouldFail(graphql, null, "You can't use 'offset' and 'after' at the same time");
        }

        [Test]
        public void DeclaredVariableWithoutValueTest()
        {
            const string graphql = "query mytest($id: Int) { epc (id: $id) { urn } }";

            ParseShouldFail(graphql, null, "Variable [$id] is used in the query, but doesn't have a value");
        }

        [Test]
        public void FragmentWithUnknownTypeTest()
        {
            var graphql = @"
{ epcs { ... frag} }
fragment frag on Arggh { urn }
".Trim();

            ParseShouldFail(graphql, null, "Unknown type: Arggh");
        }

        [Test]
        public void UnknownEntityTest()
        {
            const string graphql = "{ Arggh { urn } }";

            ParseShouldFail(graphql, null, "Unknown entity: Arggh");
        }

        [Test]
        public void UnknownFieldTest()
        {
            const string graphql = "{ epcs { urn arggh id } }";

            ParseShouldFail(graphql, null, "Unknown field: epc.arggh");
        }

        [Test]
        public void UndeclaredVariableTest()
        {
            const string graphql = "{ epcs(id: $Arggh) { urn } }";

            ParseShouldFail(graphql, null, "Variable [$Arggh] is not declared");
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
