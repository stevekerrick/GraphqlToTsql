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
                BadgeDef.Instance,
                OrderDef.Instance,
                OrderDetailDef.Instance,
                ProductDef.Instance,
                SellerDef.Instance,
                SellerBadgeDef.Instance,
                SellerProductTotalDef.Instance
            };
        }
    }
}
