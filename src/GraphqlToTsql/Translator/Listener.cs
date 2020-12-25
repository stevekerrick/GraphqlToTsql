using Antlr4.Runtime;
using GraphqlToTsql.CodeGen;
using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    public class Listener : GqlBaseListener
    {
        private readonly QueryTree _qt;
        private string _fragmentName;

        public Listener(
            IEntityList entityList,
            Dictionary<string, object> variableValues)
        {
            _qt = new QueryTree(entityList, variableValues);
        }

        public QueryTree GetQueryTree()
        {
            return _qt;
        }

        public override void ExitVariableDefinition(GqlParser.VariableDefinitionContext context)
        {
            var name = context.variable().children[1].GetText();
            var type = context.type().GetText();
            Value defaultValue = null;
            if (context.defaultValue() != null)
            {
                var defaultValueContext = context.defaultValue().value();
                defaultValue = new Value(defaultValueContext);
            }

            _qt.Variable(name, type, defaultValue, new Context(context));
        }

        public override void ExitFragmentName(GqlParser.FragmentNameContext context)
        {
            _fragmentName = context.NAME().GetText();
        }

        // This is the best place to initialize a new Fragment definition
        public override void ExitTypeCondition(GqlParser.TypeConditionContext context)
        {
            var type = context.typeName().NAME().GetText();
            _qt.BeginFragment(_fragmentName, type, new Context(context));
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

            _qt.Field(alias, name, new Context(context));
        }

        public override void ExitFragmentSpread(GqlParser.FragmentSpreadContext context)
        {
            var name = context.fragmentName().NAME().GetText();
            _qt.UseFragment(name);
        }

        public override void ExitArgument(GqlParser.ArgumentContext context)
        {
            var name = context.NAME().GetText();

            var valueOrVariableContext = context.valueOrVariable();
            if (valueOrVariableContext.variable() != null)
            {
                var variableName = valueOrVariableContext.variable().children[1].GetText();
                _qt.Argument(name, variableName, new Context(context));
            }
            else
            {
                var value = new Value(valueOrVariableContext.value());
                _qt.Argument(name, value);
            }
        }

        public override void ExitOperationDefinition(GqlParser.OperationDefinitionContext context)
        {
            var name = context.NAME();
            if (name != null)
            {
                _qt.SetOperationName(name.GetText());
            }
        }

        #region Unsupported GraphQL features

        public override void EnterInlineFragment(GqlParser.InlineFragmentContext context)
        {
            throw InvalidRequestException.Unsupported("Inline Fragments", context);
        }

        public override void EnterDirective(GqlParser.DirectiveContext context)
        {
            throw InvalidRequestException.Unsupported("Directives", context);
        }

        #endregion
    }
}
