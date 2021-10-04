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

        public Value(ValueType valueType, object rawValue)
        {
            ValueType = valueType;
            RawValue = rawValue;
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

        public static Value ScalarValueFromParse(GqlParser.ValueContext valueContext, Type expectedEnumType = null)
        {
            if (valueContext == null)
            {
                return new Value(ValueType.Null, null);
            }

            var nullValueContext = valueContext.nullValue();
            if (nullValueContext != null)
            {
                return new Value(ValueType.Null, null);
            }

            //var enumValueContext = valueContext.enumValue();
            //if (expectedEnumType != null)
            //{
            //    if (enumValueContext == null)
            //    {
            //        var expectedValues = string.Join(", ", Enum.GetNames(expectedEnumType));
            //        throw new InvalidRequestException(ErrorCode.V13, $"Expected an unquoted enum value, one of ({expectedValues})", new Context(valueContext));
            //    }

            //    var enumValueString = enumValueContext.GetText();
            //    try
            //    {
            //        Enum.Parse(expectedEnumType, enumValueString);
            //        ValueType = ValueType.String;
            //        RawValue = enumValueString;
            //        return;
            //    }
            //    catch
            //    {
            //        var expectedValues = string.Join(", ", Enum.GetNames(expectedEnumType));
            //        throw new InvalidRequestException(ErrorCode.V13, $"Expected one of ({expectedValues})", new Context(valueContext));
            //    }
            //}

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

                return new Value(ValueType.String, stringValue);
            }

            var intValueContext = valueContext.intValue();
            if (intValueContext != null)
            {
                return new Value(ValueType.Int, long.Parse(intValueContext.GetText()));
            }

            var floatValueContext = valueContext.floatValue();
            if (floatValueContext != null)
            {
                return new Value(ValueType.Float, decimal.Parse(floatValueContext.GetText()));
            }

            var boolValueContext = valueContext.booleanValue();
            if (boolValueContext != null)
            {
                return new Value(ValueType.Boolean, bool.Parse(boolValueContext.GetText()));
            }

            var listValueContext = valueContext.listValue();
            if (listValueContext != null)
            {
                throw new InvalidRequestException(ErrorCode.V11, "List values are not allowed here", new Context(valueContext));
            }

            var objectValueContext = valueContext.objectValue();
            if (objectValueContext != null)
            {
                throw new InvalidRequestException(ErrorCode.V12, "Object values are not allowed here", new Context(valueContext));
            }

            var enumValueContext = valueContext.enumValue();
            if (enumValueContext != null)
            {
                throw new InvalidRequestException(ErrorCode.V13, "Enum values are not allowed here", new Context(valueContext));
            }

            throw new InvalidRequestException(ErrorCode.V14, "Unexpected value type", new Context(valueContext));
        }

        public static Value FromStringValue(ValueType valueType, string stringValue)
        {
            var rawValue = (object)null;

            // This is only used by our internally-constructed Cursors, so we can trust Parse
            switch (valueType)
            {
                case ValueType.Null:
                    rawValue = null;
                    break;
                case ValueType.String:
                    rawValue = stringValue;
                    break;
                case ValueType.Int:
                    rawValue = long.Parse(stringValue);
                    break;
                case ValueType.Float:
                    rawValue = decimal.Parse(stringValue);
                    break;
                case ValueType.Boolean:
                    rawValue = bool.Parse(stringValue); ;
                    break;
                default:
                    throw new Exception($"Unsupported ValueType: {valueType}");
            }

            return new Value(valueType, rawValue);
        }
    }
}
