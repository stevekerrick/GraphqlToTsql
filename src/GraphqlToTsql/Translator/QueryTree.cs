using GraphqlToTsql.Entities;
using System.Collections.Generic;
using System.Linq;

namespace GraphqlToTsql.Translator
{
    public class QueryTree
    {
        private Dictionary<string, object> _graphqlParameters;
        private List<EntityBase> _entityList;
        private Dictionary<string, Value> _variables;
        private Term _term;
        private Term _parent;

        public Term TopTerm { get; private set; }
        public Dictionary<string, Term> Fragments;
        public string OperationName { get; set; }

        public QueryTree()
        {
        }

        public void Initialize(Dictionary<string, object> graphqlParameters, List<EntityBase> entityList)
        {
            _graphqlParameters = graphqlParameters;
            _entityList = entityList;
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
            if (_graphqlParameters != null && _graphqlParameters.ContainsKey(name))
            {
                value = new Value(_graphqlParameters[name]);
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
            var field = LookupEntity(type);
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
                field = LookupEntity(name);
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

        private Field LookupEntity(string name)
        {
            var entity = _entityList.FirstOrDefault(_ => _.Name == name);
            if (entity != null)
            {
                return Entities.Field.Row(entity, name, null);
            }

            entity = _entityList.FirstOrDefault(_ => _.PluralName == name);
            if (entity != null)
            {
                return Entities.Field.Set(entity, name, null);
            }

            return null;
        }
    }
}
