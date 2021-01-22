using GraphqlToTsql.Translator;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GraphqlToTsql.Entities
{
    [DebuggerDisplay("{Name,nq}")]
    public abstract class EntityBase
    {
        private List<Field> _fields;

        public abstract string Name { get; }
        public virtual string PluralName => $"{Name}s";
        public virtual string DbTableName { get; }
        public abstract string PrimaryKeyFieldName { get; }
        public virtual string VirtualEntitySql { get; }
        public List<Field> Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = BuildFieldList();
                }
                return _fields;
            }
        }

        protected abstract List<Field> BuildFieldList();

        public Field GetField(string name, Context context = null)
        {
            // Look for a native field
            var field = Fields.FirstOrDefault(_ => _.Name == name);
            if (field != null)
            {
                return field;
            }

            // Look for a Connection (where totalCount and cursors live)
            if (name.EndsWith(Constants.CONNECTION))
            {
                var setName = name.Substring(0, name.Length - Constants.CONNECTION.Length);
                var setField = Fields.FirstOrDefault(_ => _.Name == setName && _.FieldType == FieldType.Set);
                if (setField != null)
                {
                    field = Field.Connection(setField);
                    return field;
                }
            }

            // If this is a Connection entity, allow filtering arguments on Edges.Node
            if (GetType() == typeof(ConnectionEntity))
            {
                var edgesField = GetField(Constants.EDGES);
                var nodeField = edgesField.Entity.GetField(Constants.NODE);
                return nodeField.Entity.GetField(name, context);
            }

            throw new InvalidRequestException($"Unknown field: {Name}.{name}", context);
        }

        public virtual Field PrimaryKeyField => GetField(PrimaryKeyFieldName);
    }
}
