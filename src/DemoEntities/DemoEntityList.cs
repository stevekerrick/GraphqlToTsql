using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoEntities
{
    // TODO: Refactor so that the user's app provides only the list of entities
    public class DemoEntityList : List<EntityBase>, IEntityList
    {
        public DemoEntityList()
        {
            Add(DispositionDef.Instance);
            Add(EpcDef.Instance);
            Add(LocationDef.Instance);
            Add(LotDef.Instance);
            Add(ProductDef.Instance);
        }

        public Field Find(string name)
        {
            var entity = this.FirstOrDefault(_ => _.Name == name);
            if (entity != null)
            {
                return Field.Row(entity, name, null);
            }

            entity = this.FirstOrDefault(_ => _.PluralName == name);
            if (entity != null)
            {
                return Field.Set(entity, name, null);
            }

            throw new Exception($"Unknown type: {name}");
        }
    }
}
