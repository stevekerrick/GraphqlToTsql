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
                    var token = arrayValueContext.Start;
                    throw new InvalidRequestException("Arrays Not supported", new Context(valueContext));
            }
        }

        //TODO: Detect and reject complex rawValues
        public Value(object rawValue)
        {
            if (rawValue == null)
            {
                ValueType = ValueType.None;
                RawValue = null;
                return;
            }

            var rawValueString = rawValue.ToString();
            if (string.IsNullOrEmpty(rawValueString))
            {
                ValueType = ValueType.None;
                RawValue = null;
                return;
            }

            if (decimal.TryParse(rawValueString, out var numberValue))
            {
                ValueType = ValueType.Number;
                RawValue = numberValue;
            }
            else if (bool.TryParse(rawValueString, out var boolValue))
            {
                ValueType = ValueType.Boolean;
                RawValue = boolValue;
            }
            else
            {
                ValueType = ValueType.String;
                RawValue = rawValueString;
            }
        }
    }

    public enum ValueType
    {
        None,
        Number,
        String,
        Boolean
    }
}
