using GraphqlToTsql.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Translator
{
    /// <summary>
    /// The GraphQL variables are submitted as JSON. The RawValueConverter converts
    /// the raw JSON deserialized values into the expected types.
    /// </summary>
    internal interface IRawValueConverter
    {
        Value Convert(ValueType valueType, object rawValue);
    }

    internal class RawValueConverter : IRawValueConverter
    {
        public Value Convert(ValueType valueType, object rawValue)
        {
            if (rawValue == null)
            {
                return new Value(ValueType.Null, null);
            }

            if (valueType == ValueType.OrderByExp)
            {
                return ConvertOrderByExp(rawValue);
            }

            // The base types are all scalars, and the ValueType for them is assigned based on the value.
            // This might look strange, since we aren't enforcing the ValueType in the request,
            // but there is auto-type-conversion done when the resulting value is used, and if the
            // value is of the wrong type a valication error is thrown at that time.
            return ConvertBaseType(rawValue);
        }

        private Value ConvertBaseType(object rawValue)
        {
            var typeName = rawValue.GetType().Name;

            switch (typeName)
            {
                case "String":
                    return new Value(ValueType.String, (string)rawValue);

                case "Int32":
                    return new Value(ValueType.Int, (long)(int)rawValue);
                case "Int64":
                    return new Value(ValueType.Int, (long)rawValue);

                case "Single":
                case "Double":
                case "Decimal":
                    var stringValue = rawValue.ToString();
                    var decimalValue = decimal.Parse(stringValue);
                    if (decimalValue % 1.0m == 0.0m)
                    {
                        // If there's no fractional part, convert to Int
                        return new Value(ValueType.Int, (long)decimalValue);
                    }

                    return new Value(ValueType.Float, decimalValue);

                case "Boolean":
                    return new Value(ValueType.Boolean, (bool)rawValue);

                default:
                    throw new InvalidRequestException(ErrorCode.V14, $"Unsupported value type, value=[{rawValue}], type=[{typeName}]");
            }
        }

        // e.g. { "name": "desc" }
        private Value ConvertOrderByExp(object rawValue)
        {
            if (rawValue == null)
            {
                return new Value(ValueType.Null, null);
            }

            var valueString = JsonConvert.SerializeObject(rawValue);
            var jobject = (JObject)JsonConvert.DeserializeObject(valueString);

            if (jobject.Count == 1)
            {
                var jproperty = jobject.First as JProperty;
                if (jproperty != null)
                {
                    var fieldName = jproperty.Name;

                    var value = jproperty.Value.ToString();
                    if (Enum.TryParse(value, out OrderByEnum orderByEnum))
                    {
                        var orderByExp = new OrderByExp { FieldName = fieldName, OrderByEnum = orderByEnum };
                        return new Value(ValueType.OrderByExp, orderByExp);
                    }
                }
            }

            throw new InvalidRequestException(ErrorCode.V30, $"Invalid {Constants.ORDER_BY} value. Try something like {{ id: desc }}.");

        }

    }
}
