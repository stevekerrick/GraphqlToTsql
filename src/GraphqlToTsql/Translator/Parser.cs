using Antlr4.Runtime;
using GraphqlToTsql.CodeGen;
using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GraphqlToTsql.Translator
{
    public interface IParser
    {
        ParseResult ParseGraphql(string graphql, Dictionary<string, object> graphqlParameters, List<EntityBase> entityList);
    }

    public class Parser : IParser
    {
        private readonly IListener _listener;

        public Parser(
            IListener listener)
        {
            _listener = listener;
        }

        public ParseResult ParseGraphql(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            List<EntityBase> entityList)
        {
            // Set up code-generated GqlParser
            var stream = new AntlrInputStream(graphql);
            var lexer = new GqlLexer(stream);
            var tokenStream = new CommonTokenStream(lexer);

            var outputSb = new StringBuilder();
            var errorSb = new StringBuilder();

            using (var outputStringWriter = new StringWriter(outputSb))
            using (var errorStringWriter = new StringWriter(errorSb))
            {
                try
                {
                    // Perform the parse/translation
                    _listener.Initialize(graphqlParameters, entityList);
                    var parser = new GqlParser(tokenStream, outputStringWriter, errorStringWriter);
                    parser.AddParseListener(_listener);
                    parser.document();

                    // Exit if the parse was unsuccessful
                    var errorMessage = errorSb.ToString();
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        return new ParseResult { ParseError = errorMessage };
                    }

                    // We don't know how ParseOutput is used, so bail out if we see some
                    var parseOutput = outputSb.ToString();
                    if (!string.IsNullOrWhiteSpace(parseOutput))
                    {
                        throw new Exception($"Uh-oh - parser returned some Output: {parseOutput}");
                    }

                    // Parse was successful
                    var parseResult = _listener.GetResult();
                    return parseResult;
                }
                catch (InvalidRequestException e)
                {
                    return new ParseResult { ParseError = e.Message };
                }
            }
        }
    }
}
