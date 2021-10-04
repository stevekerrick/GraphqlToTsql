using FluentAssertions;
using GraphqlToTsql.Translator;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class OrderByValueTests
    {
        private readonly object _cityDesc = new { city = "DESC" };
        private readonly object _postalCodeAsc = new { postalCode = "ASC" };

        [Test]
        public void OrderByValue_SingleColumn_Json()
        {
            var variables = new Dictionary<string, object> { { "orderBy", _cityDesc } };
            var jsonString = JsonConvert.SerializeObject(variables);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            var orderByValue = OrderByValue.FromRawValue(dict["orderBy"]);

            orderByValue.Should().BeEquivalentTo(
                new
                {
                    Fields = new[]
                    {
                        new { FieldName = "city", OrderByEnum = OrderByEnum.DESC }
                    }
                });
        }

        [Test]
        public void OrderByValue_SingleColumn_AnonymousObject()
        {
            var orderByValue = OrderByValue.FromRawValue(_cityDesc);

            orderByValue.Should().BeEquivalentTo(
                new
                {
                    Fields = new[]
                    {
                        new { FieldName = "city", OrderByEnum = OrderByEnum.DESC }
                    }
                });
        }

        [Test]
        public void OrderByValue_MultipleColumns_Json()
        {
            var columnsObject = new object[] { _cityDesc, _postalCodeAsc };
            var variables = new Dictionary<string, object> { { "orderBy", columnsObject } };
            var jsonString = JsonConvert.SerializeObject(variables);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            var orderByValue = OrderByValue.FromRawValue(dict["orderBy"]);

            orderByValue.Should().BeEquivalentTo(
                new
                {
                    Fields = new[]
                    {
                        new { FieldName = "city", OrderByEnum = OrderByEnum.DESC },
                        new { FieldName = "postalCode", OrderByEnum = OrderByEnum.ASC },
                    }
                });
        }

        [Test]
        public void OrderByValue_MultipleColumns_AnonymousObject()
        {
            var columnsObject = new object[] { _cityDesc, _postalCodeAsc };

            var orderByValue = OrderByValue.FromRawValue(columnsObject);

            orderByValue.Should().BeEquivalentTo(
                new
                {
                    Fields = new[]
                    {
                        new { FieldName = "city", OrderByEnum = OrderByEnum.DESC },
                        new { FieldName = "postalCode", OrderByEnum = OrderByEnum.ASC },
                    }
                });
        }

        [Test]
        public void OrderByValue_Null()
        {
            var orderByValue = OrderByValue.FromRawValue(null);

            Assert.IsNull(orderByValue);
        }

        [TestCaseSource(nameof(InvalidRawObjects))]
        public void OrderByValue_InvalidRawObjects(object invalidRawObject)
        {
            Action act = () => OrderByValue.FromRawValue(invalidRawObject);

            act.Should().Throw<InvalidRequestException>();
        }

        static object[] InvalidRawObjects =
        {
            new { city = "not-asc-or-desc"},
            new { city = "asc", postal = "desc"}, // Multiple columns must use array syntax
            "city",
            100,
            new object[] { new { city = "desc" }, new { postal = "happy" } },
            new object[0]
        };
    }
}
