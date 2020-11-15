using System.Collections.Generic;
using GraphqlToSql.Transpiler.Entities;

namespace GraphqlToSql.Transpiler.Transpiler
{
    public class Term
    {
        public Term Parent { get; private set; }
        public List<Term> Children { get; private set; }
        public Field Field { get; private set; }
        public string Name { get; private set; }
        public TermType TermType { get; private set; }
        public Arguments Arguments { get; }
        private static int _tableAliasSeq;
        private string _tableAlias;

        private Term()
        {
            Children = new List<Term>();
            Arguments = new Arguments();
        }

        public static Term TopLevel()
        {
            return new Term
            {
                TermType = TermType.TopLevel,
            };
        }

        public Term(Term parent, Field field, string name) : this()
        {
            Parent = parent;
            Field = field;
            Name = name;

            TermType = field.FieldType ==
                FieldType.Scalar ? TermType.Scalar
                : field.FieldType == FieldType.Row ? TermType.Item
                : TermType.List;
        }

        public string TableAlias()
        {
            if (_tableAlias == null)
            {
                _tableAlias = $"t{++_tableAliasSeq}";
            }
            return _tableAlias;
        }

        public string FullPath()
        {
            var path = Name;
            var parent = Parent;
            while (parent.TermType != TermType.TopLevel)
            {
                path = parent.Name + "." + path;
                parent = parent.Parent;
            }
            return path;
        }

        public void AddArgument(string name, Value value)
        {
            Arguments.Add(Field, name, value);
        }
    }

    public enum TermType
    {
        TopLevel,
        Scalar,
        Item,
        List
    }
}