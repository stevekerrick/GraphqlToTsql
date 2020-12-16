using Antlr4.Runtime;
using GraphqlToTsql.CodeGen;
using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GraphqlToTsql.Translator
{
    public class GraphqlTranslator
    {
        private readonly IEntityList _entityList;

        public GraphqlTranslator(IEntityList entityList)
        {
            _entityList = entityList;
        }

        public TranslateResult Translate(string graphQl, Dictionary<string, object> variableValues)
        {
            // Construct the parser
            var stream = new AntlrInputStream(graphQl);
            var lexer = new GqlLexer(stream);
            var tokenStream = new CommonTokenStream(lexer);

            var outputSb = new StringBuilder();
            var outputStringWriter = new StringWriter(outputSb);

            var errorSb = new StringBuilder();
            var errorStringWriter = new StringWriter(errorSb);

            var parser = new GqlParser(tokenStream, outputStringWriter, errorStringWriter);

            // Perform the parse/translation
            var listener = new Listener(_entityList, variableValues);
            parser.AddParseListener(listener);
            parser.document();

            // Exit if the parse was unsuccessful
            var errorMessage = errorSb.ToString();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return new TranslateResult { ParseError = errorMessage };
            }

            // We don't know how ParseOutput is used, so bail out if we see some
            var parseOutput = outputSb.ToString();
            if (!string.IsNullOrWhiteSpace(parseOutput))
            {
                throw new Exception($"Uh-oh - parser returned some Output: {parseOutput}");
            }

            // Parse was successful. Now perform the translation.
            var queryTree = listener.GetQueryTree();
            var builder = new TsqlBuilder();
            var query = builder.Build(queryTree);
            var result = new TranslateResult { Tsql = query };

            return result;
        }
    }
}
