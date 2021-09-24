using GraphqlToTsql.Translator;
using NUnit.Framework;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class ValueTests
    {
        #region ValueType + String constructor tests

        [TestCaseSource(nameof(ValueTypePlusStringTestCases))]
        public void ValueTypePlusStringTest(ValueType valueType, string stringValue, object expectedRawValue)
        {
            var value = Value.FromStringValue(valueType, stringValue);

            Assert.AreEqual(valueType, value.ValueType);
            Assert.AreEqual(expectedRawValue, value.RawValue);
        }

        private static IEnumerable<TestCaseData> ValueTypePlusStringTestCases
        {
            get
            {
                yield return new TestCaseData(ValueType.Null, null, null);
                yield return new TestCaseData(ValueType.String, "test me", "test me");
                yield return new TestCaseData(ValueType.Int, "-123456789012", -123456789012L);
                yield return new TestCaseData(ValueType.Float, "12345.678901", 12345.678901M);
                yield return new TestCaseData(ValueType.Boolean, "true", true);
            }
        }

        #endregion

        #region ValueType + Value constructor tests

        [TestCaseSource(nameof(ValueTypePlusValueTestCases))]
        public void ValueTypePlusValueTest(ValueType valueType, object rawValue, ValueType expectedValueType, object expectedRawValue)
        {
            var rawValueConverter = new RawValueConverter();
            var startingValue = rawValueConverter.Convert(ValueType.Unknown, rawValue);

            var resultingValue = new Value(valueType, startingValue, () => "unexpected error");

            Assert.AreEqual(expectedValueType, resultingValue.ValueType);
            Assert.AreEqual(expectedRawValue, resultingValue.RawValue);
        }

        private static IEnumerable<TestCaseData> ValueTypePlusValueTestCases
        {
            get
            {
                // Case 1: Either type is null
                yield return new TestCaseData(ValueType.Int, null, ValueType.Null, null);
                yield return new TestCaseData(ValueType.Null, 100, ValueType.Int, 100L);

                // Case 2: Type matches expected
                yield return new TestCaseData(ValueType.Int, 200, ValueType.Int, 200L);
                yield return new TestCaseData(ValueType.Float, 300.1, ValueType.Float, 300.1M);
                yield return new TestCaseData(ValueType.String, "beta", ValueType.String, "beta");
                yield return new TestCaseData(ValueType.Boolean, true, ValueType.Boolean, true);

                // Case 3: Int can auto-promote to Float
                yield return new TestCaseData(ValueType.Float, 400, ValueType.Float, 400M);
            }
        }

        #endregion
    }
}
