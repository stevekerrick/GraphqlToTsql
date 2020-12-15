using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphqlToTsql.Translator
{
    public class TsqlBuilder
    {
        private readonly StringBuilder _sb;
        private int _indent;
        private Sequence _aliasSequence;
        private Dictionary<string, Term> _fragments;

        public TsqlBuilder()
        {
            _sb = new StringBuilder(2048);
            _aliasSequence = new Sequence();
        }

        public string Build(QueryTree tree)
        {
            _fragments = tree.Fragments;

            if (!string.IsNullOrEmpty(tree.OperationName))
            {
                Emit("-------------------------------");
                Emit($"-- Operation: {tree.OperationName}");
                Emit("-------------------------------");
                Emit("");
            }

            BuildSelectClause(tree.TopTerm);

            Emit("");
            Emit($"{FOR_JSON}{UNWRAP_ITEM}");

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

        private void ProcessFragmentField(Term parent, Term term)
        {
            // Look up the Fragment Definition
            var fragmentName = term.Name;
            if (!_fragments.ContainsKey(fragmentName))
            {
                throw new Exception($"Fragment is not defined: {fragmentName}");
            }
            var fragment = _fragments[fragmentName];

            // Type check
            if (fragment.Field.Entity != parent.Field.Entity)
            {
                throw new Exception($"Fragment is not defined: {fragmentName}");
            }

            // Copy the fragment subquery because Terms have state
            foreach (var child in fragment.Children)
            {
                parent.Children.Add(child.Clone(parent));
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
