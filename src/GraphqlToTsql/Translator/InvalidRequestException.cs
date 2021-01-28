using Antlr4.Runtime;
using GraphqlToTsql.Translator;
using System;

namespace GraphqlToTsql.Translator
{
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException(string message) : base(message)
        {
        }

        public InvalidRequestException(string message, Context context)
            : base(context == null
                  ? message
                  : $"{message} [\"{context.Text}\", line {context.Line}, column {context.Column}]")
        {
        }

        public static InvalidRequestException Unsupported(string unsupportedFeatures, ParserRuleContext parserRuleContext)
        {
            var context = new Context(parserRuleContext);
            return new InvalidRequestException($"{unsupportedFeatures} are not supported", context);
        }
    }
}
