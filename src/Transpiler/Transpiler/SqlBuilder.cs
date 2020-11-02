using GraphqlToSql.Transpiler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphqlToSql.Transpiler.Transpiler
{
    public class SqlBuilder
    {
        private readonly StringBuilder _sb;
        private readonly Stack<Term> _stack;
        private Term _term;
        private Term _parent;

        public SqlBuilder()
        {
            _sb = new StringBuilder(2048);
            _stack = new Stack<Term>();
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
            //Console.WriteLine("BeginQuery");

            if (_parent == null)
            {
                _parent = Term.TopLevel();
            }
            else
            {
                _stack.Push(_parent);
                _parent = _term;
            }

            _term = null;
        }

        public void EndQuery()
        {
            //Console.WriteLine("EndQuery");

            if (_parent.TermType == TermType.TopLevel)
            {
                EmitTopLevelQuery();
            }
            else
            {
                EmitQuery();
                _term = _parent;
                _parent = _stack.Pop();
            }
        }

        public void Field(string name)
        {
            //Console.WriteLine($"Field: {name}");

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

            _term = new Term(_parent, field, name);
            _parent.Children.Add(_term);
        }

        #region SQL generation logic

        private const string TAB = "  ";
        private const string COMMA_TAB = ", ";

        private void EmitTopLevelQuery()
        {
            Emit("SELECT");
            var tab = TAB;
            foreach (var term in _parent.Children)
            {
                Emit(tab, $"{term.CteName()}Json AS {term.Name}");
                tab = COMMA_TAB;
            }
            Emit($"FROM {string.Join(", ", _parent.Children.Select(_ => _.CteName()))}");
            Emit("FOR JSON AUTO, INCLUDE_NULL_VALUES");
        }

        private void EmitQuery()
        {
            Emit($"WITH {_parent.CteName()}({_parent.CteName()}Json) AS (");

            Emit(TAB, "SELECT");
            var tab = TAB;
            foreach (var term in _parent.Children)
            {
                Emit(tab + TAB, $"{term.Field.DbColumnName} AS {term.Name}");
                tab = COMMA_TAB;
            }
            Emit(TAB, $"FROM {_parent.Field.Entity.DbTableName}");
            Emit(TAB, "FOR JSON AUTO, INCLUDE_NULL_VALUES");

            Emit(")");
        }

        private void Emit(string line)
        {
            Emit("", line);
        }

        private void Emit(string tab, string line)
        {
            _sb.AppendLine($"{tab}{line}");
        }

        #endregion
    }
}
