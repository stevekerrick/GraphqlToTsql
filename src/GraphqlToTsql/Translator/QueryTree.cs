using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphqlToTsql.Translator
{
    public class QueryTree
    {
        private readonly IEntityList _entityList;

        public Term TopTerm { get; private set; }
        public Dictionary<string, Term> Fragments;
        public string OperationName { get; set; }
        private Term _term;
        private Term _parent;
        private Dictionary<string, object> _variableValues;
        private Dictionary<string, Value> _variables;

        public QueryTree(
            IEntityList entityList,
            Dictionary<string, object> variableValues)
        {
            _entityList = entityList;

            _variableValues = variableValues;
            _variables = new Dictionary<string, Value>();
            Fragments = new Dictionary<string, Term>();
        }

        public void SetOperationName(string name)
        {
            OperationName = name;
        }

        public void Variable(string name, string type, Value value, Context context)
        {
            // See if there's a matching VariableValue
            if (_variableValues != null && _variableValues.ContainsKey(name))
            {
                value = new Value(_variableValues[name]);
            }

            if (value == null)
            {
                throw new InvalidRequestException($"Variable [${name}] is used in the query, but doesn't have a value", context);
            }

            _variables[name] = value;
            value.VariableName = name;
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

        public void BeginFragment(string name, string type, Context context)
        {
            var field = _entityList.Find(type);
            if (field == null)
            {
                throw new InvalidRequestException($"Unknown type: {type}", context);
            }

            _parent = Term.TopLevel();
            _term = new Term(_parent, field, type);
            _parent.Children.Add(_term);

            Fragments[name] = _term;
        }

        public void Field(string alias, string name, Context context)
        {
            Field field;

            if (_parent.TermType == TermType.TopLevel)
            {
                field = _entityList.Find(name);
                if (field == null)
                {
                    throw new InvalidRequestException($"Unknown entity: {name}", context);
                }
            }
            else
            {
                field = _parent.Field.Entity.GetField(name);
                if (field == null)
                {
                    throw new InvalidRequestException($"Unknown field: {_parent.Field.Entity.Name}.{name}", context);
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

        public void Argument(string name, string variableName, Context context)
        {
            if (!_variables.ContainsKey(variableName))
            {
                throw new InvalidRequestException($"Variable [${variableName}] is not declared", context);
            }

            _term.AddArgument(name, _variables[variableName]);
        }
    }
}
