using NUnit.Framework;
using GraphqlToSql.Transpiler.Transpiler;

namespace GraphqlToSql.TranspilerTests
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
            const string graphQl = @"
{
  codes {
    id {}
  }
}
";

            Check(graphQl, "mismatched input '}'");
        }

        private static void Check(string graphQl, string expectedError)
        {
            var translator = new Translator();
            var result = translator.Translate(graphQl);

            Assert.IsFalse(result.IsSuccessful, "The parse was successful, but was expected to fail");
            Assert.IsTrue(result.ParseError.Contains(expectedError), $"Mismatching error message: {result.ParseError}");
        }
    }
}
