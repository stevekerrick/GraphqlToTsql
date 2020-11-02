using System.Collections.Generic;
using GraphqlToSql.Transpiler.Entities;

namespace GraphqlToSql.Transpiler.Transpiler
{
    public class Term
    {
        public Term ParentTerm { get; private set; }
        public List<Term> Children { get; private set; }
        public Field Field { get; private set; }
        public string Name { get; private set; }
        public TermType TermType { get; private set; }
        private string _cteName;

        private static int _cteSeq;

        private Term()
        {
        }

        public static Term TopLevel()
        {
            return new Term
            {
                TermType = TermType.TopLevel,
                Children = new List<Term>()
            };
        }

        public Term(Term parentTerm, Field field, string name)
        {
            ParentTerm = parentTerm;
            Field = field;
            Name = name;
            Children = new List<Term>();

            TermType = field.FieldType ==
                FieldType.Scalar ? TermType.Scalar
                : field.FieldType == FieldType.Row ? TermType.Item
                : TermType.List;
        }

        public string CteName()
        {
            if (_cteName == null)
            {
                _cteName = $"cte{++_cteSeq}";
            }
            return _cteName;
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
