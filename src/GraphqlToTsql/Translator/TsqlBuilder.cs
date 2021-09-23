using GraphqlToTsql.Entities;
using GraphqlToTsql.Introspection;
using GraphqlToTsql.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphqlToTsql.Translator
{
    internal interface ITsqlBuilder
    {
        TsqlResult Build(ParseResult parseResult);
    }

    internal class TsqlBuilder : ITsqlBuilder
    {
        private List<EntityBase> _entityList;
        private EmptySetBehavior _emptySetBehavior;
        private readonly StringBuilder _sb;
        private int _indent;
        private AliasSequence _aliasSequence;
        private Dictionary<string, Term> _fragments;
        private Dictionary<string, object> _tsqlParameters;
        private List<string> _typesCheckedForCte;
        private bool _hasCte;
        private IntrospectionData _introspectionData;

        public TsqlBuilder(List<EntityBase> entityList, EmptySetBehavior emptySetBehavior)
        {
            _entityList = entityList;
            _emptySetBehavior = emptySetBehavior;
            _sb = new StringBuilder(2048);
            _aliasSequence = new AliasSequence();
            _tsqlParameters = new Dictionary<string, object>();
            _typesCheckedForCte = new List<string>();
        }

        public TsqlResult Build(ParseResult parseResult)
        {
            try
            {
                return ProcessParseResult(parseResult);
            }
            catch (InvalidRequestException e)
            {
                return new TsqlResult
                {
                    Error = e.Message,
                    ErrorCode = e.ErrorCode
                };
            }
        }

        private TsqlResult ProcessParseResult(ParseResult parseResult)
        {
            _fragments = parseResult.Fragments;

            ProcessDirectives(parseResult.RootTerm);
            foreach (var fragmentTerm in _fragments.Values)
            {
                ProcessDirectives(fragmentTerm);
            }

            if (!string.IsNullOrEmpty(parseResult.OperationName))
            {
                Emit("-------------------------------");
                Emit($"-- Operation: {parseResult.OperationName}");
                Emit("-------------------------------");
                Emit("");
            }

            BuildCommonTableExpressions(parseResult.RootTerm);
            foreach (var fragmentTerm in _fragments.Values)
            {
                BuildCommonTableExpressions(fragmentTerm);
            }

            BuildSelectClause(parseResult.RootTerm);

            Emit("");
            Emit($"{FOR_JSON}{UNWRAP_ITEM};");

            return new TsqlResult
            {
                Tsql = _sb.ToString(),
                TsqlParameters = _tsqlParameters
            };
        }

        // Terms that have Directive as their first child
        private void ProcessDirectives(Term term)
        {
            if (term.Children.Count == 0)
            {
                return;
            }

            var firstChild = term.Children[0];
            if (firstChild.TermType == TermType.Directive)
            {
                var include =
                    (firstChild.Name == Constants.INCLUDE_DIRECTIVE && firstChild.Arguments.If) ||
                    (firstChild.Name == Constants.SKIP_DIRECTIVE && !firstChild.Arguments.If);

                // The directive indicates to remove the term and exit
                if (!include)
                {
                    term.Parent.Children.Remove(term);
                    return;
                }

                // The directive indicates to keep the term. So we remove just the directive
                term.Children.Remove(firstChild);
            }

            // Check the children
            var children = new List<Term>();
            children.AddRange(term.Children);
            foreach (var child in children)
            {
                ProcessDirectives(child);
            }
        }

        private void BuildCommonTableExpressions(Term term)
        {
            var entity = term.Field?.Entity;
            if (entity != null && !_typesCheckedForCte.Contains(entity.Name))
            {
                _typesCheckedForCte.Add(entity.Name);

                if (entity.SqlDefinition != null)
                {
                    // The entity has a hardcoded CTE Select statement
                    EmitCte(entity.DbTableName, entity.SqlDefinition);
                }
                else if (IntrospectionEntityList.All().Contains(entity))
                {
                    // Introspection types always use custom SQL
                    if (_introspectionData == null)
                    {
                        _introspectionData = new IntrospectionData(_entityList);
                    }
                    EmitCte(entity.DbTableName, _introspectionData.GetCteSql(entity.Name));
                }
            }

            foreach (var child in term.Children)
            {
                BuildCommonTableExpressions(child);
            }
        }

        private void EmitCte(string tableName, string sqlDefinition)
        {
            var cteAnnouncement = _hasCte ? "," : "WITH";
            var sqlLines = sqlDefinition.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            Emit($"{cteAnnouncement} [{tableName}] AS (");
            foreach (var sqlLine in sqlLines)
            {
                Emit(TAB, sqlLine);
            }
            Emit(")");
            Emit("");

            _hasCte = true;
        }

        private void BuildSubquery(Term term)
        {
            // Enforce MaxPageSize
            if (term.TermType == TermType.List && term.Field.Entity.MaxPageSize != null)
            {
                var max = term.Field.Entity.MaxPageSize.Value;
                if (term.Arguments.First == null)
                {
                    throw new InvalidRequestException(ErrorCode.V23, $"Paging is required with {term.Name}. Use argument 'first: {max}' for the initial page, and 'first'/'offset' or 'first'/'after' for subsequent pages.");
                }
                if (term.Arguments.First.Value > max)
                {
                    throw new InvalidRequestException(ErrorCode.V24, $"The max page size for {term.Name} is {max}");
                }
            }

            // Decide whether to add logic for non-nullable lists (by default TSQL returns these as NULL instead of [])
            var wrapNonNullableList = term.TermType == TermType.List && (term.Field.IsNullable == IsNullable.No || _emptySetBehavior == EmptySetBehavior.EmptyArray);
            var nonNullableListPrefix = wrapNonNullableList ? "ISNULL (" : "";
            var nonNullableListSuffix = wrapNonNullableList ? ", '[]')" : "";

            // Wrap the subquery in a JSON_QUERY
            var separator = term.IsFirstChild ? TAB : COMMA_TAB;
            Emit("");
            Emit(TAB, $"-- {term.FullPath()} ({term.TableAlias(_aliasSequence)})");
            Emit(separator, $"{nonNullableListPrefix}JSON_QUERY ((");
            Indent();

            // Build the SQL for the subquery
            BuildSelectClause(term);
            if (term.Field.FieldType != FieldType.Connection &&
                term.Field.FieldType != FieldType.Node)
            {
                Emit(FromClause(term));
                Emit(WhereClause(term));
                EmitOrderByClause(term);
            }

            // Unwrap the JSON_QUERY
            Emit($"{FOR_JSON}{(term.TermType == TermType.Item ? UNWRAP_ITEM : "")})){nonNullableListSuffix} AS [{term.Name}]");
            Outdent();
        }

        private void BuildSelectClause(Term query)
        {
            Emit("SELECT");

            // Turn each Term to SQL. The list of Children can grow when Fragments are used,
            // so we can't use a foreach.
            var i = 0;
            while (i < query.Children.Count)
            {
                var term = query.Children[i];
                ProcessField(query, term);
                i++;
            }
        }

        private void ProcessField(Term parent, Term term)
        {
            // Fragments are special, and are detected by TermType
            if (term.TermType == TermType.Fragment)
            {
                ProcessFragmentField(parent, term);
                return;
            }

            // Processed based on FieldType
            switch (term.Field.FieldType)
            {
                case FieldType.Column:
                case FieldType.Cursor:
                    ProcessScalarField(term);
                    break;
                case FieldType.TotalCount:
                    ProcessTotalCountField(term);
                    break;
                case FieldType.Row:
                case FieldType.Set:
                case FieldType.Connection:
                case FieldType.Edge:
                case FieldType.Node:
                    BuildSubquery(term);
                    break;
                default:
                    throw new NotImplementedException($"Unexpected FieldType: {term.Field.FieldType}");
            }
        }

        private void ProcessFragmentField(Term parent, Term term)
        {
            // Look up the Fragment Definition
            var fragmentName = term.Name;
            if (!_fragments.ContainsKey(fragmentName))
            {
                throw new InvalidRequestException(ErrorCode.V25, $"Fragment is not defined: {fragmentName}");
            }
            var fragment = _fragments[fragmentName];

            // Type check
            if (fragment.Field.Entity != parent.Field.Entity)
            {
                throw new InvalidRequestException(ErrorCode.V26, $"Fragment {fragmentName} is defined for {fragment.Field.Entity.EntityType}, not {parent.Field.Entity.EntityType}");
            }

            // Copy the fragment subquery because Terms have state
            foreach (var child in fragment.Children)
            {
                parent.Children.Add(child.Clone(parent));
            }
        }

        private void ProcessScalarField(Term term)
        {
            var alias = term.Parent.TableAlias(_aliasSequence);
            var separator = term.IsFirstChild ? TAB : COMMA_TAB;

            if (term.Field.TemplateFunc != null)
            {
                Emit(separator, $"({term.Field.TemplateFunc(alias)}) AS [{term.Name}]");
            }
            else
            {
                Emit(separator, $"{alias}.[{term.Field.DbColumnName}] AS [{term.Name}]");
            }
        }

        private void ProcessTotalCountField(Term term)
        {
            var tableAlias = _aliasSequence.Next();
            var separator = term.IsFirstChild ? TAB : COMMA_TAB;
            var fromClause = FromClause(term.Parent, tableAlias);
            var whereClause = WhereClause(term.Parent, tableAlias);
            Emit(separator, $"(SELECT COUNT(1) {fromClause} {whereClause}) AS [{term.Name}]");
        }

        private string FromClause(Term term, string tableAlias = null)
        {
            tableAlias = tableAlias ?? term.TableAlias(_aliasSequence);

            if (term.Field.TemplateFunc != null)
            {
                var parentTableAlias = term.ParentForJoin.TableAlias(_aliasSequence);
                return $"FROM ({term.Field.TemplateFunc(parentTableAlias)}) {tableAlias}";
            }
            return $"FROM [{term.Field.Entity.DbTableName}] {tableAlias}";
        }

        private string WhereClause(Term term, string childTableAlias = null)
        {
            // Collect the where criteria for Row/List fields
            var whereParts = new List<string>();
            if (term.Field.Join != null)
            {
                var parentField = term.Field.Join.ParentFieldFunc();
                var parentTableAlias = term.ParentForJoin.TableAlias(_aliasSequence);
                var childField = term.Field.Join.ChildFieldFunc();
                childTableAlias = childTableAlias ?? term.TableAlias(_aliasSequence);
                whereParts.Add($"{parentTableAlias}.[{parentField.DbColumnName}] = {childTableAlias}.[{childField.DbColumnName}]");
            }

            // Collect the where criteria in the argument filters
            var filters = term.Arguments.Filters;
            if (filters.Count > 0)
            {
                childTableAlias = childTableAlias ?? term.TableAlias(_aliasSequence);
                foreach (var filter in filters)
                {
                    var lhs = filter.Field.TemplateFunc == null
                        ? $"{childTableAlias}.[{filter.Field.DbColumnName}]"
                        : $"({filter.Field.TemplateFunc(childTableAlias)})";

                    var wherePart = filter.Value.TsqlValue == null
                        ? $"{lhs} IS NULL"
                        : $"{lhs} = @{RegisterTsqlParameter(filter)}";

                    whereParts.Add(wherePart);
                }
            }

            // Where criteria for cursor
            if (term.Arguments.After != null)
            {
                var entity = term.Field.Entity;
                var cursorData = CursorUtility.DecodeCursor(term.Arguments.After, entity.DbTableName);
                var filter = new Arguments.Filter(entity.SinglePrimaryKeyFieldForPaging, cursorData.Value);
                childTableAlias = childTableAlias ?? term.TableAlias(_aliasSequence);
                whereParts.Add($"{childTableAlias}.[{filter.Field.DbColumnName}] > @{RegisterTsqlParameter(filter)}");
            }

            // Build the complete WHERE clause
            var whereClause = (string)null;
            if (whereParts.Count > 0)
            {
                whereClause = $"WHERE {string.Join(" AND ", whereParts)}";
            }

            return whereClause;
        }

        private void EmitOrderByClause(Term term)
        {
            var hasPaging = term.Arguments.First != null || term.Arguments.Offset != null || term.Arguments.After != null;
            if (!hasPaging && term.OrderBy == null)
            {
                return;
            }

            var columns = new List<string>();

            // If the query specified OrderBy, set up those column sorts first
            if (term.OrderBy != null)
            {
                foreach (var orderByField in term.OrderBy.Fields)
                {
                    columns.Add(FormatOrderByColumn(term, orderByField.Field, orderByField.OrderByEnum));
                }
            }

            // Add PK columns to the OrderBy
            var defaultOrderByEnum = term.OrderBy == null ? OrderByEnum.asc : term.OrderBy.Fields[0].OrderByEnum;
            var entity = term.Field.Entity;
            foreach (var pkField in entity.PrimaryKeyFields)
            {
                if (term.OrderBy != null && term.OrderBy.Fields.Any(_ => _.Field == pkField))
                {
                    continue;
                }

                columns.Add(FormatOrderByColumn(term, pkField, defaultOrderByEnum));
            }

            // Build the ORDER BY SQL
            var orderBy = string.Join(", ", columns);
            Emit($"ORDER BY {orderBy}");

            // If there is Paging, build the OFFSET SQL
            if (hasPaging)
            {
                var offset = term.Arguments.Offset.GetValueOrDefault(0);
                var first = term.Arguments.First;

                Emit($"OFFSET {offset} ROWS");
                if (first != null)
                {
                    Emit($"FETCH FIRST {first} ROWS ONLY");
                }
            }
        }

        private string FormatOrderByColumn(Term term, Field field, OrderByEnum orderByEnum)
        {
            var qualifiedColumnName = $"{term.TableAlias(_aliasSequence)}.[{field.DbColumnName}]";

            return orderByEnum == OrderByEnum.asc ? qualifiedColumnName : $"{qualifiedColumnName} {orderByEnum.ToString().ToUpper()}";
        }

        private string RegisterTsqlParameter(Arguments.Filter filter)
        {
            if (filter.Value.TsqlParameterName != null)
            {
                return filter.Value.TsqlParameterName;
            }

            // Find a good name for this TsqlParameter
            var fieldName = filter.Value.VariableName ?? filter.Field.Name;
            var tsqlParameterName = $"{fieldName}";
            var i = 1;

            while (_tsqlParameters.ContainsKey(tsqlParameterName))
            {
                i++;
                tsqlParameterName = $"{fieldName}{i}";
            }

            filter.Value.TsqlParameterName = tsqlParameterName;
            _tsqlParameters[tsqlParameterName] = filter.Value.TsqlValue;
            return tsqlParameterName;
        }

        #region SQL helpers

        private const string TAB = "  ";
        private const string COMMA_TAB = ", ";
        private const string FOR_JSON = "FOR JSON PATH, INCLUDE_NULL_VALUES";
        private const string UNWRAP_ITEM = ", WITHOUT_ARRAY_WRAPPER";

        private void Emit(string line)
        {
            Emit("", line);
        }

        private void Emit(string tab, string line)
        {
            if (line != null)
            {
                var indent = new String(' ', Math.Max(_indent, 0));
                _sb.AppendLine($"{indent}{tab}{line}".TrimEnd());
            }
        }

        private void Indent()
        {
            _indent += 4;
        }
        private void Outdent()
        {
            _indent -= 4;
        }

        #endregion
    }
}
