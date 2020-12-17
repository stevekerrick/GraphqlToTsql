using GraphqlToTsql.CodeGen;
using System;

namespace GraphqlToTsql.Translator
{
    public class Value
    {
        public ValueType ValueType { get; }
        public object RawValue { get; }

        //public string ValueString { get; } // Includes '' around string values

        public Value(GqlParser.ValueContext valueContext)
        {
            switch (valueContext)
            {
                case GqlParser.NumberValueContext numberValueContext:
                    ValueType = ValueType.Number;
                    RawValue = decimal.Parse(numberValueContext.GetText());
                    break;
                case GqlParser.StringValueContext stringValueContext:
                    ValueType = ValueType.String;
                    RawValue = stringValueContext.GetText();
                    break;
                case GqlParser.BooleanValueContext booleanValueContext:
                    ValueType = ValueType.Boolean;
                    RawValue = bool.Parse(booleanValueContext.GetText().ToLower());
                    break;
                case GqlParser.ArrayValueContext arrayValueContext:
                    var token = arrayValueContext.Start;
                    throw new Exception($"Arrays Not supported: [{token.Text}], line {token.Line}, column {token.Column}");
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
