using GraphqlToTsql.Translator;
using System;
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

            // Look for a Connection (where totalCount and cursors live). TODO: Cursors
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

            // Parse-related errors are from bad input. Others are Entity errors.
            if (context == null)
            {
                throw new Exception($"Unknown field: {Name}.{name}");
            }
            throw new InvalidRequestException($"Unknown field: {Name}.{name}", context);
        }
    }
}
