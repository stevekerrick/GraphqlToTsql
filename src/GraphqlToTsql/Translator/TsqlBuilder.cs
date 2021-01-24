using GraphqlToTsql.Entities;
using GraphqlToTsql.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphqlToTsql.Translator
{
    public interface ITsqlBuilder
    {
        TsqlResult Build(ParseResult parseResult);
    }

    public class TsqlBuilder : ITsqlBuilder
    {
        private readonly StringBuilder _sb;
        private int _indent;
        private AliasSequence _aliasSequence;
        private Dictionary<string, Term> _fragments;
        private Dictionary<string, object> _tsqlParameters;
        private List<string> _ctes;

        public TsqlBuilder()
        {
            _sb = new StringBuilder(2048);
            _aliasSequence = new AliasSequence();
            _tsqlParameters = new Dictionary<string, object>();
            _ctes = new List<string>();
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
                    TsqlError = e.Message
                };
            }
        }

        private TsqlResult ProcessParseResult(ParseResult parseResult)
        {
            _fragments = parseResult.Fragments;

            if (!string.IsNullOrEmpty(parseResult.OperationName))
            {
                Emit("-------------------------------");
                Emit($"-- Operation: {parseResult.OperationName}");
                Emit("-------------------------------");
                Emit("");
            }

            BuildCommonTableExpressions(parseResult.TopTerm);

            BuildSelectClause(parseResult.TopTerm);

            Emit("");
            Emit($"{FOR_JSON}{UNWRAP_ITEM};");

            return new TsqlResult
            {
                Tsql = _sb.ToString(),
                TsqlParameters = _tsqlParameters
            };
        }

        private void BuildCommonTableExpressions(Term term)
        {
            var entity = term.Field?.Entity;
            if (entity != null && entity.SqlDefinition != null && !_ctes.Contains(entity.Name))
            {
                var cteAnnouncement = _ctes.Count == 0 ? "WITH" : ",";
                var sqlLines = entity.SqlDefinition.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                Emit($"{cteAnnouncement} [{entity.DbTableName}] AS (");
                foreach (var sqlLine in sqlLines)
                {
                    Emit(TAB, sqlLine);
                }
                Emit(")");
                Emit("");

                _ctes.Add(entity.Name);
            }

            foreach (var child in term.Children)
            {
                BuildCommonTableExpressions(child);
            }
        }

        private void BuildSubquery(Term term)
        {
            // Wrap the subquery in a JSON_QUERY
            var separator = term.IsFirstChild ? TAB : COMMA_TAB;
            Emit("");
            Emit(TAB, $"-- {term.FullPath()} ({term.TableAlias(_aliasSequence)})");
            Emit(separator, "JSON_QUERY ((");
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
            Emit($"{FOR_JSON}{(term.TermType == TermType.Item ? UNWRAP_ITEM : "")})) AS [{term.Name}]");
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
                case FieldType.Scalar:
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
                throw new InvalidRequestException($"Fragment is not defined: {fragmentName}");
            }
            var fragment = _fragments[fragmentName];

            // Type check
            if (fragment.Field.Entity != parent.Field.Entity)
            {
                throw new InvalidRequestException($"Fragment {fragmentName} is defined for {fragment.Field.Entity.Name}, not {parent.Field.Entity.Name}");
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
                    var lhs = $"{childTableAlias}.[{filter.Field.DbColumnName}]";
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
            if (term.Arguments.First == null && term.Arguments.Offset == null && term.Arguments.After == null)
            {
                return;
            }

            var offset = term.Arguments.Offset.GetValueOrDefault(0);
            var first = term.Arguments.First;

            var entity = term.Field.Entity;
            var pks = entity.PrimaryKeyFields;
            var columns = pks.Select(pk => $"{term.TableAlias(_aliasSequence)}.[{pk.DbColumnName}]");
            var orderBy = string.Join(", ", columns);

            Emit($"ORDER BY {orderBy}");
            Emit($"OFFSET {offset} ROWS");
            if (first != null)
            {
                Emit($"FETCH FIRST {first} ROWS ONLY");
            }
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
