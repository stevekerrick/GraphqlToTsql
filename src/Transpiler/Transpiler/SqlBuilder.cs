using GraphqlToSql.Transpiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphqlToSql.Transpiler.Transpiler
{
    public class SqlBuilder
    {
        private readonly StringBuilder _sb;
        private readonly Stack<Frame> _frames;
        private Frame _frame;
        private FieldDef _field;

        public SqlBuilder()
        {
            _sb = new StringBuilder(2048);
            _frames = new Stack<Frame>();
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
            Console.WriteLine("BeginQuery");

            var isTopLevel = _frames.Count == 0;
            if (isTopLevel)
            {
                _frame = new Frame();
            }
            else
            {
                _frame = new Frame(_field);
            }
            _frames.Push(_frame);
        }

        public void EndQuery()
        {
            Console.WriteLine("EndQuery");

            if (_frame.IsTopLevel)
            {
                EmitTopLevelQuery();
            }
            else
            {
                EmitQuery();
                _frames.Pop();
                _frame = _frames.Peek();
            }
        }

        public void Field(string name)
        {
            Console.WriteLine($"Field: {name}");

            if (_frame.IsTopLevel)
            {
                _field = TopLevelFields.All.FirstOrDefault(_ => _.Name == name);
                if (_field == null)
                {
                    throw new Exception($"Query not defined for {name}");
                }
                _field = _field.Clone(_frame.Alias);
            }
            else
            {
                _field = _frame.ParentField.Entity.Fields.FirstOrDefault(_ => _.Name == name);
                if (_field == null)
                {
                    throw new Exception($"{_frame.ParentField.Entity.Name} does not have a field named {name}");
                }
            }

            _frame.Fields.Add(_field);
        }

        #region SQL generation logic

        private const string TAB = "  ";
        private const string COMMA_TAB = ", ";

        private void EmitTopLevelQuery()
        {
            Emit("SELECT");
            var tab = TAB;
            foreach (var field in _frame.Fields)
            {
                Emit(tab, $"{field.ParentFrame.Alias}Json AS {field.Name}");
                tab = COMMA_TAB;
            }
            Emit($"FROM {string.Join(", ",_frame.Fields.Select(_=>_.Name))}");
            Emit("FOR JSON AUTO, INCLUDE_NULL_VALUES");
        }

        private void EmitQuery()
        {
            Emit($"WITH {_frame.Alias}({_frame.Alias}Json) AS (");

            Emit(TAB, "SELECT");
            var tab = TAB;
            foreach (var field in _frame.Fields)
            {
                Emit(tab + TAB, $"{field.DbColumnName} AS {field.Name}");
                tab = COMMA_TAB;
            }
            Emit(TAB, $"FROM {_frame.ParentField.Entity.DbTableName}");
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

    internal class Frame
    {
        private static int _count = 0;

        private int Count { get; }
        public string Alias { get; } //TODO: See how this is used, then give it a better name
        public bool IsTopLevel => Count == 0;

        public FieldDef ParentField { get; }
        public List<FieldDef> Fields { get; set; }

        public Frame(FieldDef parentField = null)
        {
            Count = _count++;
            Alias = $"q{Count}";
            ParentField = parentField;
            Fields = new List<FieldDef>();
        }
    }
}
