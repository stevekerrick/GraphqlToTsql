using GraphqlToSql.Transpiler.Entities;
using System;
using System.Linq;
using System.Text;

namespace GraphqlToSql.Transpiler.Transpiler
{
    public class SqlBuilder
    {
        private readonly StringBuilder _sb;
        private Term _term;
        private Term _parent;
        private int _indent;

        public SqlBuilder()
        {
            _sb = new StringBuilder(2048);
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
                Indent();
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
                Emit($"FROM {_parent.Field.Entity.DbTableName} {_parent.TableAlias()}");
                EmitWhere();
                Emit($"{FOR_JSON})) AS {_parent.Name}");
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
                Emit(TAB, $"{_term.Parent.TableAlias()}.{field.DbColumnName} AS {_term.Name}");
            }
            else
            {
                Emit("");
                Emit($"-- {_term.FullPath()}");
                Emit("JSON_QUERY ((");
                Indent();
                Emit("SELECT");
            }
        }

        public void Argument(string name, Value value)
        {
            _term.AddArgument(name, value);
        }

        #region SQL generation logic

        private const string TAB = "  ";
        private const string COMMA_TAB = ", ";
        private const string FOR_JSON = "FOR JSON PATH, INCLUDE_NULL_VALUES";

        private void Emit(string line)
        {
            Emit("", line);
        }

        private void Emit(string tab, string line)
        {
            var indent = new String(' ', _indent);
            _sb.AppendLine($"{indent}{tab}{line}".TrimEnd());
        }

        private void EmitWhere()
        {
            var joinColumns = _parent.Arguments.JoinColumns;
            if (joinColumns.Count == 0) return;
            var joinSnips = joinColumns.Select(_ => $"{_.Field.DbColumnName} = {_.Value.ValueString}");
            Emit($"WHERE {string.Join(" AND ", joinSnips)}");
        }

        private void Indent()
        {
            _indent = _indent + 2;
        }
        private void Outdent()
        {
            _indent = _indent - 2;
        }

        #endregion
    }
}
