//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.8
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from c:\git\GraphqlToSql\src\Transpiler\GraphqlParser\Gql.g4 by ANTLR 4.8

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace GraphqlToSql.Transpiler.Parser.CodeGen {

using Antlr4.Runtime.Misc;
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="IGqlListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.8")]
[System.CLSCompliant(false)]
public partial class GqlBaseListener : IGqlListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.document"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDocument([NotNull] GqlParser.DocumentContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.document"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDocument([NotNull] GqlParser.DocumentContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.definition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDefinition([NotNull] GqlParser.DefinitionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.definition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDefinition([NotNull] GqlParser.DefinitionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.operationDefinition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperationDefinition([NotNull] GqlParser.OperationDefinitionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.operationDefinition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperationDefinition([NotNull] GqlParser.OperationDefinitionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.selectionSet"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSelectionSet([NotNull] GqlParser.SelectionSetContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.selectionSet"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSelectionSet([NotNull] GqlParser.SelectionSetContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.operationType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterOperationType([NotNull] GqlParser.OperationTypeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.operationType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitOperationType([NotNull] GqlParser.OperationTypeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.selection"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSelection([NotNull] GqlParser.SelectionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.selection"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSelection([NotNull] GqlParser.SelectionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.field"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterField([NotNull] GqlParser.FieldContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.field"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitField([NotNull] GqlParser.FieldContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.fieldName"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFieldName([NotNull] GqlParser.FieldNameContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.fieldName"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFieldName([NotNull] GqlParser.FieldNameContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.alias"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAlias([NotNull] GqlParser.AliasContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.alias"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAlias([NotNull] GqlParser.AliasContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.arguments"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArguments([NotNull] GqlParser.ArgumentsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.arguments"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArguments([NotNull] GqlParser.ArgumentsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.argument"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArgument([NotNull] GqlParser.ArgumentContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.argument"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArgument([NotNull] GqlParser.ArgumentContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.fragmentSpread"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFragmentSpread([NotNull] GqlParser.FragmentSpreadContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.fragmentSpread"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFragmentSpread([NotNull] GqlParser.FragmentSpreadContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.inlineFragment"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterInlineFragment([NotNull] GqlParser.InlineFragmentContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.inlineFragment"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitInlineFragment([NotNull] GqlParser.InlineFragmentContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.fragmentDefinition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFragmentDefinition([NotNull] GqlParser.FragmentDefinitionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.fragmentDefinition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFragmentDefinition([NotNull] GqlParser.FragmentDefinitionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.fragmentName"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFragmentName([NotNull] GqlParser.FragmentNameContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.fragmentName"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFragmentName([NotNull] GqlParser.FragmentNameContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.directives"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDirectives([NotNull] GqlParser.DirectivesContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.directives"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDirectives([NotNull] GqlParser.DirectivesContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDirective([NotNull] GqlParser.DirectiveContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.directive"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDirective([NotNull] GqlParser.DirectiveContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.typeCondition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterTypeCondition([NotNull] GqlParser.TypeConditionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.typeCondition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitTypeCondition([NotNull] GqlParser.TypeConditionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.variableDefinitions"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVariableDefinitions([NotNull] GqlParser.VariableDefinitionsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.variableDefinitions"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVariableDefinitions([NotNull] GqlParser.VariableDefinitionsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.variableDefinition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVariableDefinition([NotNull] GqlParser.VariableDefinitionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.variableDefinition"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVariableDefinition([NotNull] GqlParser.VariableDefinitionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.variable"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVariable([NotNull] GqlParser.VariableContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.variable"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVariable([NotNull] GqlParser.VariableContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.defaultValue"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDefaultValue([NotNull] GqlParser.DefaultValueContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.defaultValue"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDefaultValue([NotNull] GqlParser.DefaultValueContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.valueOrVariable"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterValueOrVariable([NotNull] GqlParser.ValueOrVariableContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.valueOrVariable"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitValueOrVariable([NotNull] GqlParser.ValueOrVariableContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>stringValue</c>
	/// labeled alternative in <see cref="GqlParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStringValue([NotNull] GqlParser.StringValueContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>stringValue</c>
	/// labeled alternative in <see cref="GqlParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStringValue([NotNull] GqlParser.StringValueContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>numberValue</c>
	/// labeled alternative in <see cref="GqlParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNumberValue([NotNull] GqlParser.NumberValueContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>numberValue</c>
	/// labeled alternative in <see cref="GqlParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNumberValue([NotNull] GqlParser.NumberValueContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>booleanValue</c>
	/// labeled alternative in <see cref="GqlParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBooleanValue([NotNull] GqlParser.BooleanValueContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>booleanValue</c>
	/// labeled alternative in <see cref="GqlParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBooleanValue([NotNull] GqlParser.BooleanValueContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>arrayValue</c>
	/// labeled alternative in <see cref="GqlParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArrayValue([NotNull] GqlParser.ArrayValueContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>arrayValue</c>
	/// labeled alternative in <see cref="GqlParser.value"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArrayValue([NotNull] GqlParser.ArrayValueContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.type"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterType([NotNull] GqlParser.TypeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.type"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitType([NotNull] GqlParser.TypeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.typeName"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterTypeName([NotNull] GqlParser.TypeNameContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.typeName"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitTypeName([NotNull] GqlParser.TypeNameContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.listType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterListType([NotNull] GqlParser.ListTypeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.listType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitListType([NotNull] GqlParser.ListTypeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.nonNullType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNonNullType([NotNull] GqlParser.NonNullTypeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.nonNullType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNonNullType([NotNull] GqlParser.NonNullTypeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="GqlParser.array"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArray([NotNull] GqlParser.ArrayContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="GqlParser.array"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArray([NotNull] GqlParser.ArrayContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
} // namespace GraphqlToSql.Transpiler.Parser.CodeGen
