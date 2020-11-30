using GraphqlToTsql.Translator.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphqlToTsql.Translator.Translator
{
    public class QueryTree
    {
        public Term TopTerm { get; private set; }
        public Dictionary<string, Term> Fragments;
        private Term _term;
        private Term _parent;
        private object _variableValues;
        private Dictionary<string, Value> _variables;

        public QueryTree(object variableValues)
        {
            _variableValues = variableValues;
            _variables = new Dictionary<string, Value>();
            Fragments = new Dictionary<string, Term>();
        }

        public void Variable(string name, string type, Value value)
        {
            // See if there's a matching VariableValue
            if (_variableValues != null)
            {
                var propertyInfo = _variableValues.GetType().GetProperty(name);
                if (propertyInfo != null)
                {
                    var rawVariableValue = propertyInfo.GetValue(_variableValues, null);
                    value = new Value(rawVariableValue);
                }
            }

            if (value == null)
            {
                throw new Exception($"Variable [${name}] is used in the query, but doesn't have a value");
            }

            _variables[name] = value;
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

        public void BeginFragment(string name, string type)
        {
            var field = AllEntities.Find(type);

            _parent = Term.TopLevel();
            _term = new Term(_parent, field, type);
            _parent.Children.Add(_term);

            Fragments[name] = _term;
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

        public void UseFragment(string name)
        {
            _term = Term.Fragment(_parent, name);
            _parent.Children.Add(_term);
        }

        public void Argument(string name, Value value)
        {
            _term.AddArgument(name, value);
        }

        public void Argument(string name, string variableName)
        {
            if (!_variables.ContainsKey(variableName))
            {
                throw new Exception($"Variable [${variableName}] is not declared");
            }

            _term.AddArgument(name, _variables[variableName]);
        }
    }
}
