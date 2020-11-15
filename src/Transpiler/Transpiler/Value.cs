using System;
using GraphqlToSql.Transpiler.Parser.CodeGen;

namespace GraphqlToSql.Transpiler.Transpiler
{
    public class Value
    {
        public ValueType ValueType { get; }
        public string ValueString { get; }

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
                    ValueString = stringValueContext.GetText().Replace('"', '\'');
                    break;
                case GqlParser.BooleanValueContext booleanValueContext:
                    ValueType = ValueType.Boolean;
                    var boolString = booleanValueContext.GetText().ToUpper();
                    ValueString = boolString == "TRUE" ? "1" : "0";
                    break;
                case GqlParser.ArrayValueContext arrayValueContext:
                    var token = arrayValueContext.Start;
                    throw new Exception($"Arrays Not supported: [{token.Text}], at {token.Line}:{token.Column}");
            }
        }
    }

    public enum ValueType
    {
        Number,
        String,
        Boolean,
        //Array
    }
}
