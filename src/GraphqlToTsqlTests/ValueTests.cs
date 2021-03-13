using GraphqlToTsql.Translator;
using NUnit.Framework;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class ValueTests
    {
        #region RawValue constructor tests

        [TestCaseSource(nameof(RawValueTestCases))]
        public void RawValueTest(object rawValue, ValueType expectedValueType, object expectedRawValue)
        {
            var value = new Value(rawValue);

            Assert.AreEqual(expectedValueType, value.ValueType);
            Assert.AreEqual(expectedRawValue, value.RawValue);
        }

        private static IEnumerable<TestCaseData> RawValueTestCases
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

        #endregion

        #region ValueType + String constructor tests

        [TestCaseSource(nameof(ValueTypePlusStringTestCases))]
        public void ValueTypePlusStringTest(ValueType valueType, string stringValue, object expectedRawValue)
        {
            var value = new Value(valueType, stringValue);

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
            var value = new Value(rawValue);
            var actualValue = new Value(valueType, value, () => "unexpected error");

            Assert.AreEqual(expectedValueType, actualValue.ValueType);
            Assert.AreEqual(expectedRawValue, actualValue.RawValue);
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
