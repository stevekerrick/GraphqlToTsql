using GraphqlToTsql.Entities;
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
        private Sequence _aliasSequence;
        private Dictionary<string, Term> _fragments;
        private Dictionary<string, object> _tsqlParameters;

        public TsqlBuilder()
        {
            _sb = new StringBuilder(2048);
            _aliasSequence = new Sequence();
            _tsqlParameters = new Dictionary<string, object>();
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

            BuildSelectClause(parseResult.TopTerm);

            Emit("");
            Emit($"{FOR_JSON}{UNWRAP_ITEM}");

            return new TsqlResult
            {
                Tsql = _sb.ToString(),
                TsqlParameters = _tsqlParameters
            };
        }

        private void BuildSubquery(Term term)
        {
            // Wrap the subquery in a JSON_QUERY
            var separator = term.IsFirstChild ? TAB : COMMA_TAB;
            Emit("");
            Emit(TAB, $"-- {term.FullPath()}");
            Emit(separator, "JSON_QUERY ((");
            Indent();

            // Build the SQL for the subquery
            BuildSelectClause(term);
            if (term.Field.FieldType != FieldType.Connection)
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
            if (term.TermType == TermType.Fragment)
            {
                ProcessFragmentField(parent, term);
            }
            else if (term.Field.FieldType == FieldType.Scalar)
            {
                ProcessScalarField(term);
            }
            else if (term.Field.FieldType == FieldType.TotalCount)
            {
                ProcessTotalCountField(term);
            }
            else
            {
                BuildSubquery(term);
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
            var alias = term.Parent.TableAlias(_aliasSequence);
            var separator = term.IsFirstChild ? TAB : COMMA_TAB;
            var fromClause = FromClause(term.Parent);
            var whereClause = WhereClause(term.Parent);
            Emit(separator, $"(SELECT COUNT(1) {fromClause} {whereClause}) AS [{term.Name}]");
        }

        private string FromClause(Term term)
        {
            return $"FROM [{term.Field.Entity.DbTableName}] {term.TableAlias(_aliasSequence)}";
        }

        private string WhereClause(Term term)
        {
            // Collect the join criteria for Row/List fields
            var joinSnips = new List<string>();
            if (term.Field.Join != null)
            {
                var parentField = term.Field.Join.ParentFieldFunc();
                var parentTableAlias = term.Parent.TableAlias(_aliasSequence);
                var childField = term.Field.Join.ChildFieldFunc();
                var childTableAlias = term.TableAlias(_aliasSequence);
                joinSnips.Add($"{parentTableAlias}.[{parentField.DbColumnName}] = {childTableAlias}.[{childField.DbColumnName}]");
            }

            // Collect the join criteria in the argument filters
            var filters = term.Arguments.Filters;
            if (filters.Count > 0)
            {
                var tableAlias = term.TableAlias(_aliasSequence);
                joinSnips.AddRange(filters.Select(filter => $"{tableAlias}.[{filter.Field.DbColumnName}] = @{RegisterTsqlParameter(filter)}"));
            }

            // Build the complete WHERE clause
            var whereClause = (string)null;
            if (joinSnips.Count > 0)
            {
                whereClause = $"WHERE {string.Join(" AND ", joinSnips)}";
            }

            return whereClause;
        }

        private void EmitOrderByClause(Term term)
        {
            if (term.Arguments.Offset != null || term.Arguments.First != null)
            {
                var entity = term.Field.Entity;
                var pkColumnName = entity.GetField(entity.PrimaryKeyFieldName).DbColumnName;
                Emit($"ORDER BY {term.TableAlias(_aliasSequence)}.{pkColumnName}");
                Emit($"OFFSET {term.Arguments.Offset.GetValueOrDefault(0)} ROWS");
                if (term.Arguments.First != null)
                {
                    Emit($"FETCH FIRST {term.Arguments.First} ROWS ONLY");
                }
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

            // Refine the value, if needed
            var value = filter.Value.RawValue;
            if (value != null && value is decimal)
            {
                var decimalValue = (decimal)value;
                var intValue = Convert.ToInt32(decimalValue);
                if (decimalValue - intValue == 0.0M)
                {
                    value = intValue;
                }
            }

            filter.Value.TsqlParameterName = tsqlParameterName;
            _tsqlParameters[tsqlParameterName] = value;
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
