using GraphqlToTsql.Translator;
using NUnit.Framework;
using System.Collections.Generic;
using Newtonsoft.Json;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class RawValueConverterTests
    {
        private readonly RawValueConverter _rawValueConverter = new RawValueConverter();

        [TestCaseSource(nameof(BaseTypeTestCases))]
        public void RawValue_BaseTypes(object rawValue, ValueType expectedValueType, object expectedRawValue)
        {
            var value = _rawValueConverter.Convert(ValueType.Unknown, rawValue);

            Assert.AreEqual(expectedValueType, value.ValueType);
            Assert.AreEqual(expectedRawValue, value.RawValue);
        }

        private static IEnumerable<TestCaseData> BaseTypeTestCases
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

        [Test]
        public void RawValue_JsonValue()
        {
            var variables = new Dictionary<string, object> { { "orderBy", new { city = "desc" } } };
            //var orderBy = new { city= "desc" };
            var jsonString = JsonConvert.SerializeObject(variables);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            var value = _rawValueConverter.Convert(ValueType.OrderByExp, dict["orderBy"]);

            Assert.AreEqual(ValueType.OrderByExp, value.ValueType);
            Assert.AreEqual("OrderByExp", value.RawValue.GetType().Name);

            var orderByExp = (OrderByExp)value.RawValue;
            Assert.AreEqual("city", orderByExp.FieldName);
            Assert.AreEqual(OrderByEnum.desc, orderByExp.OrderByEnum);
        }

        // TODO: Test anonymous object
        // TODO: Think -- any other formats?




    }
}
