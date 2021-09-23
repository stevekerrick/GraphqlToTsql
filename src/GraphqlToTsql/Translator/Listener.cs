using Antlr4.Runtime.Tree;
using GraphqlToTsql.CodeGen;
using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    internal interface IListener : IParseTreeListener
    {
        void Initialize(Dictionary<string, object> graphqlParameters, List<EntityBase> entityList);
        ParseResult GetResult();
    }

    internal class Listener : GqlBaseListener, IListener
    {
        private readonly IQueryTreeBuilder _qt;
        private string _fragmentName;

        public Listener(IQueryTreeBuilder queryTreeBuilder)
        {
            _qt = queryTreeBuilder;
        }

        public void Initialize(Dictionary<string, object> graphqlParameters, List<EntityBase> entityList)
        {
            _qt.Initialize(graphqlParameters, entityList);
        }

        public ParseResult GetResult()
        {
            return _qt.GetResult();
        }

        public override void ExitVariableDefinition(GqlParser.VariableDefinitionContext context)
        {
            var name = context.variable().children[1].GetText();

            var typeContext = context.type_();
            if (typeContext.listType() != null)
            {
                throw InvalidRequestException.Unsupported(ErrorCode.V03, "Array values", context);
            }
            var type = typeContext.namedType().GetText();

            var typeIsNullable = !context.type_().GetText().EndsWith("!");

            Value defaultValue = null;
            if (context.defaultValue() != null)
            {
                var defaultValueContext = context.defaultValue().value();
                defaultValue = new Value(defaultValueContext);
            }

            _qt.Variable(name, type, typeIsNullable, defaultValue, new Context(context));
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
            var aliasContext = context.alias();
            var alias = aliasContext == null ? null : aliasContext.name().GetText();

            var name = context.name().GetText();

            _qt.Field(alias, name, new Context(context));
        }

        public override void ExitArgument(GqlParser.ArgumentContext context)
        {
            var name = context.name().GetText();

            var valueOrVariableContext = context.value();
            if (valueOrVariableContext == null)
            {
                throw new InvalidRequestException(ErrorCode.V01, $"Arguments should be formed like (id: 1)", new Context(context));
            }

            // Found ORDER BY specification
            if (name == Constants.ORDER_BY)
            {
                var objectValue = ParseObjectValue(valueOrVariableContext, ErrorCode.V30);
                if (objectValue.ObjectFields.Count != 1)
                {
                    throw new InvalidRequestException(ErrorCode.V30, $"{Constants.ORDER_BY} must specify exactly one field to order by", new Context(context));
                }

                _qt.OrderBy(objectValue, new Context(context));
                return;
            }

            // The r-value is a scalar variable reference
            if (valueOrVariableContext.variable() != null)
            {
                var variableName = valueOrVariableContext.variable().children[1].GetText();
                _qt.Argument(name, variableName, new Context(context));
                return;
            }

            // The r-value is a scalar value
            var value = new Value(valueOrVariableContext);
            _qt.Argument(name, value, new Context(context));
        }

        public override void ExitOperationDefinition(GqlParser.OperationDefinitionContext context)
        {
            var name = context.name();
            if (name != null)
            {
                _qt.SetOperationName(name.GetText());
            }
        }

        public override void ExitDirectiveName(GqlParser.DirectiveNameContext context)
        {
            var name = context.name().GetText();
            _qt.BeginDirective(name, new Context(context));
        }

        public override void ExitDirective(GqlParser.DirectiveContext context)
        {
            _qt.EndDirective();
        }

        // -------------------------------------
        // Fragment stuff
        // -------------------------------------
        public override void ExitFragmentName(GqlParser.FragmentNameContext context)
        {
            _fragmentName = context.name().GetText();
        }

        // This is the best place to initialize a new Fragment definition
        public override void ExitTypeCondition(GqlParser.TypeConditionContext context)
        {
            var type = context.namedType().name().GetText();
            _qt.BeginFragment(_fragmentName, type, new Context(context));
        }

        public override void ExitFragmentSpread(GqlParser.FragmentSpreadContext context)
        {
            var name = context.fragmentName().name().GetText();
            _qt.UseFragment(name);
        }

        #region Unsupported GraphQL features

        public override void EnterInlineFragment(GqlParser.InlineFragmentContext context)
        {
            throw InvalidRequestException.Unsupported(ErrorCode.V02, "Inline Fragments", context);
        }

        #endregion

        private ObjectValue ParseObjectValue(GqlParser.ValueContext valueContext, ErrorCode errorCode)
        {
            var objectValueContext = valueContext.objectValue();
            if (objectValueContext == null)
            {
                throw new InvalidRequestException(errorCode, "An object value was expected", new Context(valueContext));
            }

            var objectValue = new ObjectValue();
            var objectFieldContexts = objectValueContext.objectField();
            foreach(var objectFieldContext in objectFieldContexts)
            {
                objectValue.ObjectFields.Add(ParseObjectField(objectFieldContext, typeof(OrderByEnum)));
            }

            return objectValue;
        }

        private ObjectField ParseObjectField(GqlParser.ObjectFieldContext objectFieldContext, Type enumTypeAllowed)
        {
            var name = objectFieldContext.name().GetText();
            var value = new Value(objectFieldContext.value(), enumTypeAllowed);
            return new ObjectField
            {
                Name = name,
                Value = value
            };
        }
    }
}
