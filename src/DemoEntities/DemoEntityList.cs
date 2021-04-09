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
                BadgeEntity.Instance,
                OrderEntity.Instance,
                OrderDetailEntity.Instance,
                ProductEntity.Instance,
                SellerEntity.Instance,
                SellerBadgeEntity.Instance,
                SellerProductTotalEntity.Instance,
                SellerTotalEntity.Instance
            };
        }
    }
}
