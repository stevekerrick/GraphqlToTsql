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
        private Term _term;
        private Term _parent;
        private int _indent;
        private Sequence _aliasSequence;

        public TsqlBuilder()
        {
            _sb = new StringBuilder(2048);
            _aliasSequence = new Sequence();
        }

        public Query GetResult()
        {
            return new Query
            {
                Command = _sb.ToString(),
                Parameters = new Query.QueryParameters() //TODO: Collect parameters
            };
        }

        public void BeginQuery()
        {
            if (_parent == null)
            {
                _parent = Term.TopLevel();
                Emit("SELECT");
            }
            else
            {
                _parent = _term;
            }

            _term = null;
        }

        public void EndQuery()
        {
            if (_parent.TermType == TermType.TopLevel)
            {
                Emit("");
                Emit(FOR_JSON);
            }
            else
            {
                Emit($"FROM {_parent.Field.Entity.DbTableName} {_parent.TableAlias(_aliasSequence)}");
                EmitWhere();
                Emit($"{FOR_JSON}{(_parent.TermType==TermType.Item?UNWRAP_ITEM:"")})) AS {_parent.Name}");
                Outdent();
                Outdent();
                _term = _parent;
                _parent = _term.Parent;
            }
        }

        public void Field(string alias, string name)
        {
            Field field;

            if (_parent.TermType == TermType.TopLevel)
            {
                field = TopLevelFields.All.FirstOrDefault(_ => _.Name == name);
                if (field == null)
                {
                    throw new Exception($"Query not defined for {name}");
                }
            }
            else
            {
                field = _parent.Field.Entity.Fields.FirstOrDefault(_ => _.Name == name);
                if (field == null)
                {
                    throw new Exception($"{_parent.Field.Entity.Name} does not have a field named {name}");
                }
            }

            _term = new Term(_parent, field, alias ?? name);
            _parent.Children.Add(_term);

            // Emit
            if (field.FieldType == FieldType.Scalar)
            {
                Emit(TAB, $"{_term.Parent.TableAlias(_aliasSequence)}.{field.DbColumnName} AS {_term.Name}");
            }
            else
            {
                var separator = _parent.Children.Count == 1 ? TAB : COMMA_TAB;
                Emit("");
                Emit(TAB, $"-- {_term.FullPath()}");
                Emit(separator, "JSON_QUERY ((");
                Indent();
                Indent();
                Emit("SELECT");
            }
        }

        public void Argument(string name, Value value)
        {
            _term.AddArgument(name, value);
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

        private void EmitWhere()
        {
            // Collect the join criteria for Row/List fields
            var joinSnips = new List<string>();
            if (_parent.Field.Join != null)
            {
                var parentField = _parent.Field.Join.ParentFieldFunc();
                var parentTableAlias = _parent.Parent.TableAlias(_aliasSequence);
                var childField = _parent.Field.Join.ChildFieldFunc();
                var childTableAlias = _parent.TableAlias(_aliasSequence);
                joinSnips.Add($"{parentTableAlias}.{parentField.DbColumnName} = {childTableAlias}.{childField.DbColumnName}");
            }

            // Collect the join criteria in the argument filters
            var filters = _parent.Arguments.Filters;
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
