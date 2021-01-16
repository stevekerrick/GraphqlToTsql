using GraphqlToTsql.CodeGen;
using System;

namespace GraphqlToTsql.Translator
{
    public class Value
    {
        public string VariableName { get; set; }
        public ValueType ValueType { get; }
        public object RawValue { get; }
        public string TsqlParameterName { get; set; }

        public Value(GqlParser.ValueContext valueContext)
        {
            if (valueContext == null)
            {
                ValueType = ValueType.Null;
                RawValue = null;
                return;
            }

            switch (valueContext)
            {
                case GqlParser.NumberValueContext numberValueContext:
                    ValueType = ValueType.Number;
                    RawValue = decimal.Parse(numberValueContext.GetText());
                    break;
                case GqlParser.StringValueContext stringValueContext:
                    var quoted = stringValueContext.GetText();
                    var unquoted = quoted.Substring(1, quoted.Length - 2);
                    ValueType = ValueType.String;
                    RawValue = unquoted;
                    break;
                case GqlParser.BooleanValueContext booleanValueContext:
                    ValueType = ValueType.Boolean;
                    RawValue = bool.Parse(booleanValueContext.GetText().ToLower());
                    break;
                case GqlParser.ArrayValueContext arrayValueContext:
                    throw new InvalidRequestException("Arrays Not supported", new Context(valueContext));
            }
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
                case "Int32":
                case "Int64":
                case "Single":
                case "Double":
                case "Decimal":
                    var stringValue = rawValue.ToString();
                    ValueType = ValueType.Number;
                    RawValue = decimal.Parse(stringValue);
                    break;

                case "String":
                    ValueType = ValueType.String;
                    RawValue = (string)rawValue;
                    break;

                case "Boolean":
                    ValueType = ValueType.Boolean;
                    RawValue = (bool)rawValue;
                    break;

                default:
                    throw new InvalidRequestException($"Unsupported value type, value=[{rawValue}], type=[{typeName}]");
            }
        }

        public Value(ValueType valueType, string stringValue)
        {
            ValueType = valueType;

            switch (valueType)
            {
                case ValueType.Null:
                    RawValue = null;
                    break;
                case ValueType.Number:
                    RawValue = decimal.Parse(stringValue);
                    break;
                case ValueType.String:
                    RawValue = stringValue;
                    break;
                case ValueType.Boolean:
                    RawValue = bool.Parse(stringValue); ;
                    break;
                default:
                    throw new Exception($"Unsupported ValueType: {valueType}");
            }
        }
    }

    public enum ValueType
    {
        Null = 1,
        Number,
        String,
        Boolean
    }
}
