using GraphqlToTsql.CodeGen;
using System;

namespace GraphqlToTsql.Translator
{
    public class Value
    {
        public ValueType ValueType { get; }
        public string ValueString { get; } // Includes '' around string values

        public Value(GqlParser.ValueContext valueContext)
        {
            switch (valueContext)
            {
                case GqlParser.NumberValueContext numberValueContext:
                    ValueType = ValueType.Number;
                    ValueString = numberValueContext.GetText();
                    break;
                case GqlParser.StringValueContext stringValueContext:
                    ValueType = ValueType.String;
                    //TODO: Fix SQL Injection problem here
                    ValueString = stringValueContext.GetText().Replace('"', '\'');
                    break;
                case GqlParser.BooleanValueContext booleanValueContext:
                    ValueType = ValueType.Boolean;
                    var boolString = booleanValueContext.GetText().ToUpper();
                    ValueString = boolString == "TRUE" ? "1" : "0";
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
                ValueType = ValueType.Unknown;
                ValueString = null;
                return;
            }

            var rawValueString = rawValue.ToString();
            if (string.IsNullOrEmpty(rawValueString))
            {
                ValueType = ValueType.Unknown;
                ValueString = null;
                return;
            }

            if (decimal.TryParse(rawValueString, out _))
            {
                ValueType = ValueType.Number;
                ValueString = rawValueString;
            }
            else if (bool.TryParse(rawValueString, out var boolValue))
            {
                ValueType = ValueType.Boolean;
                ValueString = boolValue ? "1" : "0";
            }
            else
            {
                ValueType = ValueType.String;
                //TODO: Fix SQL Injection problem here
                ValueString = $"'{rawValueString}'";
            }
        }
    }

    public enum ValueType
    {
        Unknown,
        ID,
        Number,
        String,
        Boolean
    }
}
