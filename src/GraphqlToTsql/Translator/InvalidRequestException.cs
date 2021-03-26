using Antlr4.Runtime;
using System;

namespace GraphqlToTsql.Translator
{
    internal class InvalidRequestException : Exception
    {
        public ErrorCode ErrorCode { get; }

        public InvalidRequestException(ErrorCode errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public InvalidRequestException(ErrorCode errorCode, string message, Context context)
            : base(context == null
                  ? message
                  : $"{message} [\"{context.Text}\", line {context.Line}, column {context.Column}]")
        {
            ErrorCode = errorCode;
        }

        public static InvalidRequestException Unsupported(ErrorCode errorCode, string unsupportedFeatures, ParserRuleContext parserRuleContext)
        {
            var context = new Context(parserRuleContext);
            return new InvalidRequestException(errorCode, $"{unsupportedFeatures} are not supported", context);
        }
    }

    public enum ErrorCode
    {
        NoError = 0,
        V01, // Malformed argument
        V02, // Inline fragment
        V03, // Array argument
        V04, // Variable of unknown type
        V05, // Variable has no value
        V06, // Unknown entity
        V07, // Unknown field
        V08, // Unknown directive
        V09, // Variable is not defined
        V10, // Fragment is for unknown type
        V11, // Arrays are not supported
        V12, // Values of type Object are not supported
        V13, // Enum values are not supported
        V14, // Unsupported value type
        V15, // Value doesn't match declared type
        V16, // Arguments are used in a place they're not supported
        V17, // Offset and After can not be used together
        V18, // Unsupported directive argument
        V19, // A null-value filter argument can not be used on a non-nullable field
        V20, // Int value expected
        V21, // String value expected
        V22, // Boolean value expected
        V23, // Paging is required for the list
        V24, // Maximum page size exceeded
        V25, // Fragment is not defined
        V26, // Fragment is defined for a different type
        V27, // Cursor-based paging not supported for the list
        V28, // Cursor is invalid
        V29, // Malformed Graphql 
        E01, // Database error
    }
}
