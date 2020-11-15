using System.Collections.Generic;

namespace GraphqlToTsql.Translator.Entities
{
    public abstract class EntityBase
    {
        public abstract string Name { get; }
        public virtual string DbTableName { get; }
        public virtual string PluralName => $"{Name}s";
        public List<Field> Fields { get; protected set; }

        // public string SortField => Fields[0].Name; //TODO
    }
}
