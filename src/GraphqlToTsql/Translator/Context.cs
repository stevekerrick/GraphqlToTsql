using Antlr4.Runtime;

namespace GraphqlToTsql.Translator
{
    public class Context
    {
        public string Text { get; }
        public int Line { get; }
        public int Column { get; }

        public Context(ParserRuleContext context)
        {
            var token = context.Start;
            Text = token.Text;
            Line = token.Line;
            Column = token.Column;
        }
    }
}
