using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphqlToTsql.Translator.Entities
{
    public static class AllEntities
    {
        public static List<EntityBase> All => new List<EntityBase>{
            EpcDef.Instance, LocationDef.Instance, LotDef.Instance, ProductDef.Instance
        };

        public static Field Find(string name)
        {
            var entity = All.FirstOrDefault(_ => _.Name == name);
            if (entity != null)
            {
                return Field.Row(entity, name, null);
            }

            // This now looks like a bad idea. The query looks up things by Term name
            // in the regular query, and Type Name in Fragments
            // entity = All.FirstOrDefault(_ => _.PluralName == name);
            // if (entity != null)
            // {
            //     return Field.Set(entity, name, null);
            // }

            throw new Exception($"Unknown type: {name}");
        }
    }
}
