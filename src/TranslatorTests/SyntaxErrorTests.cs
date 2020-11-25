using GraphqlToTsql.Translator;
using NUnit.Framework;

namespace GraphqlToTsql.TranslatorTests
{
    [TestFixture]
    public class SyntaxErrorTests
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

        private static void Check(string graphQl, string expectedError)
        {
            var translator = new GraphqlTranslator();
            var result = translator.Translate(graphQl, null);

            Assert.IsFalse(result.IsSuccessful, "The parse was successful, but was expected to fail");
            Assert.IsTrue(result.ParseError.Contains(expectedError), $"Mismatching error message: {result.ParseError}");
        }
    }
}
