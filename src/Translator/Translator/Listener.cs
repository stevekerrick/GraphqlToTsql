using Antlr4.Runtime;
using GraphqlToTsql.GraphqlParser.CodeGen;
using System;

namespace GraphqlToTsql.Translator.Translator
{
    public class Listener : GqlBaseListener
    {
        private readonly QueryTree _qt;

        public Listener()
        {
            _qt = new QueryTree();
        }

        public QueryTree GetQueryTree()
        {
            return _qt;
        }

        public override void EnterSelectionSet(GqlParser.SelectionSetContext context)
        {
            _qt.BeginQuery();
        }

        public override void ExitSelectionSet(GqlParser.SelectionSetContext context)
        {
            _qt.EndQuery();
        }

        public override void ExitFieldName(GqlParser.FieldNameContext context)
        {
            string name, alias;
            var aliasContext = context.alias();

            if (aliasContext != null)
            {
                alias = aliasContext.NAME()[0].GetText();
                name = aliasContext.NAME()[1].GetText();
            }
            else
            {
                alias = null;
                name = context.NAME().GetText();
            }

            _qt.Field(alias, name);
        }

        public override void ExitArgument(GqlParser.ArgumentContext context)
        {
            var name = context.NAME().GetText();

            var valueOrVariableContext = context.valueOrVariable();
            if (valueOrVariableContext.variable() != null)
            {
                Unsupported(valueOrVariableContext.variable().Start);
            }

            var value = new Value(valueOrVariableContext.value());
            _qt.Argument(name, value);
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
