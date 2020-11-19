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
            ProcessTree(tree.TopTerm);
            return _sb.ToString();
        }

        private void ProcessTree(Term topTerm)
        {
            Emit("SELECT");

            foreach (var child in topTerm.Children)
            {
                ProcessField(topTerm, child);
            }

            Emit("");
            Emit(FOR_JSON);
        }

        private void ProcessField(Term parent, Term term)
        {
            if (term.Field.FieldType == FieldType.Scalar)
            {
                ProcessScalarField(term);
            }
            else
            {
                ProcessFooField(parent, term);
            }
        }

        private void ProcessScalarField(Term term)
        {
            var alias = term.Parent.TableAlias(_aliasSequence);
            Emit(TAB, $"{alias}.{term.Field.DbColumnName} AS {term.Name}");
        }

        private void ProcessFooField(Term parent, Term term)
        {
            var separator = parent.Children.Count == 1 ? TAB : COMMA_TAB;
            Emit("");
            Emit(TAB, $"-- {term.FullPath()}");
            Emit(separator, "JSON_QUERY ((");
            Indent();
            Indent();
            Emit("SELECT");

            ProcessQuery(term);
        }


        private void ProcessQuery(Term parent)
        {
            foreach (var term in parent.Children)
            {
                ProcessField(parent, term);
            }

            Emit($"FROM {parent.Field.Entity.DbTableName} {parent.TableAlias(_aliasSequence)}");
            EmitWhere(parent);
            Emit($"{FOR_JSON}{(parent.TermType == TermType.Item ? UNWRAP_ITEM : "")})) AS {parent.Name}");
            Outdent();
            Outdent();
        }

        private void EmitWhere(Term parent)
        {
            // Collect the join criteria for Row/List fields
            var joinSnips = new List<string>();
            if (parent.Field.Join != null)
            {
                var parentField = parent.Field.Join.ParentFieldFunc();
                var parentTableAlias = parent.Parent.TableAlias(_aliasSequence);
                var childField = parent.Field.Join.ChildFieldFunc();
                var childTableAlias = parent.TableAlias(_aliasSequence);
                joinSnips.Add($"{parentTableAlias}.{parentField.DbColumnName} = {childTableAlias}.{childField.DbColumnName}");
            }

            // Collect the join criteria in the argument filters
            var filters = parent.Arguments.Filters;
            if (filters.Count > 0)
            {
                joinSnips.AddRange(filters.Select(_ => $"{_.Field.DbColumnName} = {_.Value.ValueString}"));
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
            _indent += 2;
        }
        private void Outdent()
        {
            _indent -= 2;
        }

        #endregion
    }
}
