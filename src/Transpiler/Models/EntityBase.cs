using System.Collections.Generic;

namespace GraphqlToSql.Transpiler.Models
{
    public abstract class EntityBase
    {
        public abstract string Name { get; }
        public virtual string DbTableName { get; }
        public virtual string PluralName => $"{Name}s";
        public List<FieldDef> Fields { get; protected set; }

        // public string SortField => Fields[0].Name; //TODO
    }
}
