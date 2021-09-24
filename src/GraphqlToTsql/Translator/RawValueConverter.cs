using System;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Translator
{
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
                throw new NotImplementedException();
            }

            return ConvertScalar(rawValue);
        }

        private Value ConvertScalar(object rawValue)
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

    }
}
