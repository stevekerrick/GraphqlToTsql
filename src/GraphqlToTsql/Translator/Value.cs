using GraphqlToTsql.CodeGen;
using System;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Translator
{
    internal class Value
    {
        public string VariableName { get; set; }
        public ValueType ValueType { get; }
        public object RawValue { get; }
        public string TsqlParameterName { get; set; }

        // Value that can be used for TSQL Parameters
        public object TsqlValue
        {
            get
            {
                // For booleans return 0/1 (because they're type BIT in the database)
                if (ValueType == ValueType.Boolean)
                {
                    var boolValue = (bool)RawValue;
                    return boolValue ? 1 : 0;
                }

                return RawValue;
            }
        }

        public Value(GqlParser.ValueContext valueContext)
        {
            if (valueContext == null)
            {
                ValueType = ValueType.Null;
                RawValue = null;
                return;
            }

            var nullValueContext = valueContext.nullValue();
            if (nullValueContext != null)
            {
                ValueType = ValueType.Null;
                RawValue = null;
                return;
            }

            var stringValueContext = valueContext.stringValue();
            if (stringValueContext != null)
            {
                var stringValue = (string)null;
                if (stringValueContext.STRING() != null)
                {
                    var quotedString = stringValueContext.STRING().GetText();
                    stringValue = quotedString.Substring(1, quotedString.Length - 2);
                }
                else if (stringValueContext.BLOCK_STRING() != null)
                {
                    var tripleQuotedString = stringValueContext.BLOCK_STRING().GetText();
                    stringValue = tripleQuotedString.Substring(3, tripleQuotedString.Length - 6);
                }

                ValueType = ValueType.String;
                RawValue = stringValue;
                return;
            }

            var intValueContext = valueContext.intValue();
            if (intValueContext != null)
            {
                ValueType = ValueType.Int;
                RawValue = long.Parse(intValueContext.GetText());
                return;
            }

            var floatValueContext = valueContext.floatValue();
            if (floatValueContext != null)
            {
                ValueType = ValueType.Float;
                RawValue = decimal.Parse(floatValueContext.GetText());
                return;
            }

            var boolValueContext = valueContext.booleanValue();
            if (boolValueContext != null)
            {
                ValueType = ValueType.Boolean;
                RawValue = bool.Parse(boolValueContext.GetText());
                return;
            }

            var listValueContext = valueContext.listValue();
            if (listValueContext != null)
            {
                throw new InvalidRequestException(ErrorCode.V11, "List values are not supported", new Context(valueContext));
            }

            var objectValueContext = valueContext.objectValue();
            if (objectValueContext != null)
            {
                throw new InvalidRequestException(ErrorCode.V12, "Object values are not supported", new Context(valueContext));
            }

            var enumValueContext = valueContext.enumValue();
            if (enumValueContext != null)
            {
                throw new InvalidRequestException(ErrorCode.V13, "Enum values are not supported", new Context(valueContext));
            }

            throw new InvalidRequestException(ErrorCode.V14, "Unexpected value type", new Context(valueContext));
        }

        public Value(object rawValue)
        {
            if (rawValue == null)
            {
                ValueType = ValueType.Null;
                RawValue = null;
                return;
            }

            var typeName = rawValue.GetType().Name;

            switch (typeName)
            {
                case "String":
                    ValueType = ValueType.String;
                    RawValue = (string)rawValue;
                    break;

                case "Int32":
                    ValueType = ValueType.Int;
                    RawValue = (long)(int)rawValue;
                    break;
                case "Int64":
                    ValueType = ValueType.Int;
                    RawValue = (long)rawValue;
                    break;

                case "Single":
                case "Double":
                case "Decimal":
                    var stringValue = rawValue.ToString();
                    var decimalValue = decimal.Parse(stringValue);
                    if (decimalValue % 1.0m == 0.0m)
                    {
                        // If there's no fractional part, convert to Int
                        ValueType = ValueType.Int;
                        RawValue = (long)decimalValue;
                    }
                    else
                    {
                        ValueType = ValueType.Float;
                        RawValue = decimalValue;
                    }
                    break;

                case "Boolean":
                    ValueType = ValueType.Boolean;
                    RawValue = (bool)rawValue;
                    break;

                default:
                    throw new InvalidRequestException(ErrorCode.V14, $"Unsupported value type, value=[{rawValue}], type=[{typeName}]");
            }
        }

        public Value(ValueType valueType, string stringValue)
        {
            ValueType = valueType;

            // This is only used by our internally-constructed Cursors, so we can trust Parse
            switch (valueType)
            {
                case ValueType.Null:
                    RawValue = null;
                    break;
                case ValueType.String:
                    RawValue = stringValue;
                    break;
                case ValueType.Int:
                    RawValue = long.Parse(stringValue);
                    break;
                case ValueType.Float:
                    RawValue = decimal.Parse(stringValue);
                    break;
                case ValueType.Boolean:
                    RawValue = bool.Parse(stringValue); ;
                    break;
                default:
                    throw new Exception($"Unsupported ValueType: {valueType}");
            }
        }

        public Value(ValueType expectedValueType, Value value, Func<string> errorMessageFunc)
        {
            // Allow the value if it matches the expected type, or if it is null
            if (value.ValueType == expectedValueType ||
                expectedValueType == ValueType.Null ||
                value.ValueType == ValueType.Null)
            {
                ValueType = value.ValueType;
                RawValue = value.RawValue;
                VariableName = value.VariableName;
                return;
            }

            // Allow Int => Float
            if (value.ValueType == ValueType.Int && expectedValueType == ValueType.Float)
            {
                ValueType = ValueType.Float;
                RawValue = Convert.ToDecimal((long)value.RawValue);
                VariableName = value.VariableName;
                return;
            }

            var errorMessage = errorMessageFunc();
            throw new InvalidRequestException(ErrorCode.V15, errorMessage);
        }
    }
}
