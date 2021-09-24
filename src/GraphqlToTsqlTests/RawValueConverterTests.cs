using GraphqlToTsql.Translator;
using NUnit.Framework;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class RawValueConverterTests
    {
        [TestCaseSource(nameof(ScalarTestCases))]
        public void RawValueTest(object rawValue, ValueType expectedValueType, object expectedRawValue)
        {
            var rawValueConverter = new RawValueConverter();
            var value = rawValueConverter.Convert(ValueType.Unknown, rawValue);

            Assert.AreEqual(expectedValueType, value.ValueType);
            Assert.AreEqual(expectedRawValue, value.RawValue);
        }

        private static IEnumerable<TestCaseData> ScalarTestCases
        {
            get
            {
                // Null
                yield return new TestCaseData(null, ValueType.Null, null);

                // String
                yield return new TestCaseData("", ValueType.String, "");
                yield return new TestCaseData("alpha", ValueType.String, "alpha");

                // Int
                yield return new TestCaseData(100, ValueType.Int, 100L);
                yield return new TestCaseData(300L, ValueType.Int, 300L);
                yield return new TestCaseData(-100, ValueType.Int, -100L);

                // Float
                yield return new TestCaseData(200.1234F, ValueType.Float, 200.1234M);
                yield return new TestCaseData(-400.1234D, ValueType.Float, -400.1234M);
                yield return new TestCaseData(16.17M, ValueType.Float, 16.17M);

                // Boolean
                yield return new TestCaseData(true, ValueType.Boolean, true);
                yield return new TestCaseData(false, ValueType.Boolean, false);
            }
        }
    }
}
