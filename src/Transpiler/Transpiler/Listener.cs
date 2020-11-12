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
            string name, alias;
            var aliasToken = context.alias();

            if (aliasToken != null)
            {
                alias = aliasToken.NAME()[0].GetText();
                name = aliasToken.NAME()[1].GetText();
            }
            else
            {
                alias = null;
                name = context.NAME().GetText();
            }

            _sql.Field(alias, name);
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
