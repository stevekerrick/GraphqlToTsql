﻿using GraphqlToTsql.Translator;
using NUnit.Framework;
using ValueType = GraphqlToTsql.Translator.ValueType;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class ValueTests
    {
        [TestCase(100, ValueType.Int, "100")]
        [TestCase(200.1234F, ValueType.Float, "200.1234")]
        [TestCase(300L, ValueType.Int, "300")]
        [TestCase(-400.1234D, ValueType.Float, "-400.1234")]
        [TestCase(100, ValueType.Int, "100.0")]
        public void RawValueToNumberTest(object rawValue, ValueType expectedValueType, string expectedRawValueString)
        {
            var value = new Value(rawValue);

            Assert.AreEqual(expectedValueType, value.ValueType);

            var expectedRawValue = decimal.Parse(expectedRawValueString);
            Assert.AreEqual(expectedRawValue, value.RawValue);
        }

        [Test]
        public void DecimalToNumberTest()
        {
            var dec = 1234.6789M;
            var value = new Value(dec);

            Assert.AreEqual(ValueType.Float, value.ValueType);
            Assert.AreEqual(dec, value.RawValue);
        }

        [Test]
        public void NullTest()
        {
            var value = new Value(null);

            Assert.AreEqual(ValueType.Null, value.ValueType);
            Assert.IsNull(value.RawValue);
        }

    }
}
