using GraphqlToTsql.Translator.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphqlToTsql.Translator.Translator
{
    public class TsqlBuilder
    {
        private readonly StringBuilder _sb;
        private int _indent;
        private Sequence _aliasSequence;

        public TsqlBuilder()
        {
            _sb = new StringBuilder(2048);
            _aliasSequence = new Sequence();
        }

        public string Build(QueryTree tree)
        {
            BuildSelectClause(tree.TopTerm);

            Emit("");
            Emit(FOR_JSON);

            return _sb.ToString();
        }

        private void BuildSubquery(Term parent, Term subquery)
        {
            // Wrap the subquery in a JSON_QUERY
            var separator = subquery.IsFirstChild ? TAB : COMMA_TAB;
            Emit("");
            Emit(TAB, $"-- {subquery.FullPath()}");
            Emit(separator, "JSON_QUERY ((");
            Indent();

            // Build the SQL for the subquery
            BuildSelectClause(subquery);
            BuildFromClause(subquery);
            BuildWhereClause(subquery);

            // Unwrap the JSON_QUERY
            Emit($"{FOR_JSON}{(subquery.TermType == TermType.Item ? UNWRAP_ITEM : "")})) AS [{subquery.Name}]");
            Outdent();
        }

        private void BuildSelectClause(Term query)
        {
            Emit("SELECT");
            
            foreach (var term in query.Children)
            {
                ProcessField(query, term);
            }
        }

        private void ProcessField(Term parent, Term term)
        {
            if (term.Field.FieldType == FieldType.Scalar)
            {
                ProcessScalarField(term);
            }
            else
            {
                BuildSubquery(parent, term);
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

        private void BuildFromClause(Term subquery)
        {
            Emit($"FROM [{subquery.Field.Entity.DbTableName}] {subquery.TableAlias(_aliasSequence)}");
        }

        private void BuildWhereClause(Term parent)
        {
            // Collect the join criteria for Row/List fields
            var joinSnips = new List<string>();
            if (parent.Field.Join != null)
            {
                var parentField = parent.Field.Join.ParentFieldFunc();
                var parentTableAlias = parent.Parent.TableAlias(_aliasSequence);
                var childField = parent.Field.Join.ChildFieldFunc();
                var childTableAlias = parent.TableAlias(_aliasSequence);
                joinSnips.Add($"{parentTableAlias}.[{parentField.DbColumnName}] = {childTableAlias}.[{childField.DbColumnName}]");
            }

            // Collect the join criteria in the argument filters
            var filters = parent.Arguments.Filters;
            if (filters.Count > 0)
            {
                var tableAlias = parent.TableAlias(_aliasSequence);
                joinSnips.AddRange(filters.Select(_ => $"{tableAlias}.[{_.Field.DbColumnName}] = {_.Value.ValueString}"));
            }

            // Emit the complete WHERE clause
            if (joinSnips.Count > 0)
            {
                Emit($"WHERE {string.Join(" AND ", joinSnips)}");
            }
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
            var indent = new String(' ', Math.Max(_indent, 0));
            _sb.AppendLine($"{indent}{tab}{line}".TrimEnd());
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
