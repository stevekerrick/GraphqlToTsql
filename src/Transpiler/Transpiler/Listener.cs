using System;
using Antlr4.Runtime;
using GraphqlToSql.Transpiler.Parser.CodeGen;

namespace GraphqlToSql.Transpiler.Transpiler
{
    public class Listener : GqlBaseListener
    {
        private readonly SqlBuilder _sql;

        public Listener()
        {
            _sql = new SqlBuilder();
        }

        public Query GetResult()
        {
            return _sql.GetResult();
        }

        public override void EnterSelectionSet(GqlParser.SelectionSetContext context)
        {
            _sql.BeginQuery();
        }

        public override void ExitSelectionSet(GqlParser.SelectionSetContext context)
        {
            _sql.EndQuery();
        }

        public override void ExitFieldName(GqlParser.FieldNameContext context)
        {
            var fieldName = context.NAME().GetText();
            _sql.Field(fieldName);
        }

        #region Unsupported GraphQL features

        public override void EnterFragmentDefinition(GqlParser.FragmentDefinitionContext context)
        {
            Unsupported(context.Start);
        }

        private static void Unsupported(IToken token)
        {
            throw new Exception($"Not supported: [{token.Text}], at {token.Line}:{token.Column}");
        }

        #endregion
    }
}
