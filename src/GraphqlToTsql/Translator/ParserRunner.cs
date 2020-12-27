﻿using Antlr4.Runtime;
using GraphqlToTsql.CodeGen;
using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GraphqlToTsql.Translator
{
    public interface IParserRunner
    {
        ParseResult ParseGraphql(string graphQl, Dictionary<string, object> graphqlParameters, List<EntityBase> entityList);
    }

    public class ParserRunner : IParserRunner
    {
        private readonly IListener _listener;

        public ParserRunner(
            IListener listener)
        {
            _listener = listener;
        }

        public ParseResult ParseGraphql(
            string graphQl,
            Dictionary<string, object> graphqlParameters,
            List<EntityBase> entityList)
        {
            // Set up parser
            var stream = new AntlrInputStream(graphQl);
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
