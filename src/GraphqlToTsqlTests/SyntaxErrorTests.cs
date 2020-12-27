using DemoEntities;
using GraphqlToTsql;
using GraphqlToTsql.Translator;
using NUnit.Framework;
using System.Threading.Tasks;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class SyntaxErrorTests : IntegrationTestBase
    {
        [Test]
        public void SimpleStringTest()
        {
            Check("my graphQl", "mismatched input 'my'");
        }

        [Test]
        public void UnexpectedSymbolTest()
        {
            const string graphQl = "{ epcs { } }";

            Check(graphQl, "mismatched input '}'");
        }

        private void Check(string graphQl, string expectedError)
        {
            var parser = GetService<IParser>();
            var parseResult = parser.ParseGraphql(graphQl, null, DemoEntityList.All());

            Assert.IsNotNull(parseResult.ParseError, "The parse was successful, but was expected to fail");
            Assert.IsTrue(parseResult.ParseError.Contains(expectedError), $"Mismatching error message: {parseResult.ParseError}");
        }
    }
}
