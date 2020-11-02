using System;
using NUnit.Framework;
using GraphqlToSql.Transpiler.Transpiler;

namespace GraphqlToSql.TranspilerTests
{
    [TestFixture]
    public class SimpleSqlTests
    {
        [Test]
        public void QuerySimpleFieldsTest()
        {
            const string graphQl = "{ codes { id parentCodeId codeStatusId secureCode } }";
            Check(graphQl);
        }


        private static void Check(string graphQl)
        {
            var translator = new Translator();
            var result = translator.Translate(graphQl);
            Console.WriteLine("------------------------------------");

            Assert.IsTrue(result.IsSuccessful, $"The parse failed: {result.ParseError}");
            Console.WriteLine(result.Query.Command); 
        }
    }
}
