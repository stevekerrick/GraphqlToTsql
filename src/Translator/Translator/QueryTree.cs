using GraphqlToTsql.Translator.Entities;
using System;
using System.Linq;

namespace GraphqlToTsql.Translator.Translator
{
    public class QueryTree
    {
        public Term TopTerm { get; private set; }
        private Term _term;
        private Term _parent;

        public QueryTree()
        {
        }

        public void BeginQuery()
        {
            if (_parent == null)
            {
                _parent = Term.TopLevel();
                TopTerm = _parent;
            }
            else
            {
                _parent = _term;
            }

            _term = null;
        }

        public void EndQuery()
        {
            if (_parent.TermType != TermType.TopLevel)
            {
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
        }

        public void Argument(string name, Value value)
        {
            _term.AddArgument(name, value);
        }
    }
}
