using Antlr4.Runtime;
using GraphqlToSql.Transpiler.Parser.CodeGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GraphqlToSql.Transpiler.Transpiler
{
    public class Translator
    {
        public TranslateResult Translate(string graphQl)
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
            var listener = new Listener();
            parser.AddParseListener(listener);
            parser.document();

            var result = new TranslateResult
            {
                //ParseOutput = outputSb.ToString(),
                ParseError = errorSb.ToString(),
                Query = listener.GetResult()
            };

            // Temp: we don't know how ParseOutput is used
            var parseOutput = outputSb.ToString();
            if (!string.IsNullOrWhiteSpace(parseOutput))
                throw new Exception($"Uh-oh - parser returned some Output: {parseOutput}");

            return result;
        }
    }
}
