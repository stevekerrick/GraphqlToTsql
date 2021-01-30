using GraphqlToTsql.Entities;
using System.Collections.Generic;
using System.Linq;

namespace GraphqlToTsql.Translator
{
    public interface IQueryTreeBuilder
    {
        void Initialize(Dictionary<string, object> graphqlParameters, List<EntityBase> entityList);
        ParseResult GetResult();

        void Argument(string name, string variableName, Context context);
        void Argument(string name, Value value, Context context);
        void BeginFragment(string name, string type, Context context);
        void BeginQuery();
        void EndQuery();
        void Field(string alias, string name, Context context);
        void SetOperationName(string name);
        void UseFragment(string name);
        void Variable(string name, string type, Value value, Context context);
    }

    public class QueryTreeBuilder : IQueryTreeBuilder
    {
        private Dictionary<string, object> _graphqlParameters;
        private List<EntityBase> _entityList;
        private Dictionary<string, Value> _variables;
        private Term _term;
        private Term _parent;

        private string _operationName;
        private Dictionary<string, Term> _fragments;
        private Term _topTerm;

        public QueryTreeBuilder()
        {
        }

        public void Initialize(Dictionary<string, object> graphqlParameters, List<EntityBase> entityList)
        {
            _graphqlParameters = graphqlParameters;
            _entityList = entityList;
            _variables = new Dictionary<string, Value>();
            _fragments = new Dictionary<string, Term>();
        }

        public ParseResult GetResult()
        {
            return new ParseResult
            {
                OperationName = _operationName,
                Fragments = _fragments,
                TopTerm = _topTerm
            };
        }

        public void SetOperationName(string name)
        {
            _operationName = name;
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
                _topTerm = _parent;
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
            var field = LookupType(type, context);

            _parent = Term.TopLevel();
            _term = new Term(_parent, field, type);
            _parent.Children.Add(_term);

            _fragments[name] = _term;
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

        public void Argument(string name, Value value, Context context)
        {
            _term.AddArgument(name, value, context);
        }

        public void Argument(string name, string variableName, Context context)
        {
            if (!_variables.ContainsKey(variableName))
            {
                throw new InvalidRequestException($"Variable [${variableName}] is not declared", context);
            }

            _term.AddArgument(name, _variables[variableName], context);
        }

        private Field LookupType(string type, Context context)
        {
            var entity = _entityList.FirstOrDefault(_ => _.EntityType == type);
            if (entity == null)
            {
                throw new InvalidRequestException($"Unknown type: {type}", context);
            }

            return Entities.Field.Row(entity, entity.Name, null);
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