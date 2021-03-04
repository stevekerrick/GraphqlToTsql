using GraphqlToTsql.Translator;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GraphqlToTsql.Entities
{
    /// <summary>
    /// Each of the tables you want to expose in the GraphQL will need an Entity class, inheriting from EntityBase
    /// </summary>
    [DebuggerDisplay("{Name,nq}")]
    public abstract class EntityBase
    {
        private List<Field> _fields;

        /// <summary>
        /// The name to use for the entity in GraphQL queries (singlular). This is mainly used for the root query.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The plural name to use for the entity in GraphQL queries. This is mainly used for the root query.
        /// </summary>
        public virtual string PluralName => $"{Name}s";

        /// <summary>
        /// The name for the underlying SQL table
        /// </summary>
        public virtual string DbTableName { get; }

        /// <summary>
        /// The name of the entity type
        /// </summary>
        public virtual string EntityType => DbTableName;

        /// <summary>
        /// The names of the PK fields. Use the GraphQL names, not the SQL names. 
        /// An entity must have at least one PK field.
        /// PK fields are needed when the query uses paging.
        /// </summary>
        public abstract string[] PrimaryKeyFieldNames { get; }

        /// <summary>
        /// Sometimes you want to map a GraphQL entity to a SQL SELECT statement rather than an actual
        /// database table. To do that, put your SELECT statement here.
        /// When you use this entity in one of your queries, GraphqlToTsqpl puts uses your SELECT as a
        /// Common Table Expression, so regular CTE limitations apply (e.g. you aren't allowed to
        /// use an ORDER BY clause).
        /// 
        /// When using SqlDefinition, set DbTableName to the name for the CTE.
        /// </summary>
        public virtual string SqlDefinition { get; }

        /// <summary>
        /// If you set a MaxPageSize, users are forced to access lists of thei entity using paging.
        /// </summary>
        public virtual long? MaxPageSize { get; }

        internal List<Field> Fields
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

        /// <summary>
        /// You must implement this method to populate the list of entity fields.
        /// This is the hardest part of your entity mapping. You'll use the static Field
        /// factory methods.
        /// </summary>
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

            throw new InvalidRequestException($"Unknown field: {EntityType}.{name}", context);
        }

        /// <summary>
        /// Don't override this method. It's virtual so that EdgeEntity can override it.
        /// </summary>
        internal virtual List<Field> PrimaryKeyFields
        {
            get
            {
                return PrimaryKeyFieldNames
                    .Select(pk => GetField(pk))
                    .ToList();
            }
        }

        internal Field SinglePrimaryKeyFieldForPaging
        {
            get
            {
                var pks = PrimaryKeyFields;
                if (pks.Count > 1)
                {
                    throw new InvalidRequestException($"Cursor-based paging can not be used for {Name}. Try paging with First/Offset instead.");
                }
                return pks[0];
            }
        }
    }
}
