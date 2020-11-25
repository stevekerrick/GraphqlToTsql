using Antlr4.Runtime;
using GraphqlToTsql.GraphqlParser.CodeGen;
using GraphqlToTsql.Translator.Translator;
using System;
using System.IO;
using System.Text;

namespace GraphqlToTsql.Translator
{
    public class GraphqlTranslator
    {
        public TranslateResult Translate(string graphQl, object values)
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
            var listener = new Listener(values);
            parser.AddParseListener(listener);
            parser.document();

            var builder = new TsqlBuilder();
            var query = builder.Build(listener.GetQueryTree());

            var result = new TranslateResult
            {
                ParseError = errorSb.ToString(),
                Query = query
            };

            // Temp: we don't know how ParseOutput is used
            var parseOutput = outputSb.ToString();
            if (!string.IsNullOrWhiteSpace(parseOutput))
                throw new Exception($"Uh-oh - parser returned some Output: {parseOutput}");

            return result;
        }
    }
}
