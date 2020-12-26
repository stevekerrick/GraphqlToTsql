using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace DemoEntities
{
    public static class DemoEntityList
    {
        public static List<EntityBase> All()
        {
            return new List<EntityBase>
            {
                DispositionDef.Instance,
                EpcDef.Instance,
                LocationDef.Instance,
                LotDef.Instance,
                ProductDef.Instance
            };
        }
    }
}
