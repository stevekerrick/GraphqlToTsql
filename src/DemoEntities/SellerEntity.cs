using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace DemoEntities
{
    public class SellerEntity : EntityBase
    {
        public static SellerEntity Instance = new SellerEntity();

        public override string Name => "seller";
        public override string DbTableName => "Seller";
        public override string[] PrimaryKeyFieldNames => new[] { "name" };

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "distributorName", "DistributorName", ValueType.String, IsNullable.Yes, Visibility.Hidden),
                Field.Column(this, "city", "City", ValueType.String, IsNullable.Yes),
                Field.Column(this, "state", "State", ValueType.String, IsNullable.Yes),
                Field.Column(this, "postalCode", "PostalCode", ValueType.String, IsNullable.Yes),

                Field.Row(this, "distributor", new Join(
                    ()=>this.GetField("distributorName"),
                    ()=>Instance.GetField("name"))
                ),
                Field.Row(SellerTotalEntity.Instance, "sellerTotal", new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerTotalEntity.Instance.GetField("sellerName"))
                ),
                Field.CalculatedRow(this, "apexDistributor",
                    (tableAlias) => $"SELECT s.* FROM tvf_AllAncestors({tableAlias}.Name) d INNER JOIN Seller s ON d.Name = s.Name AND s.DistributorName IS NULL"
                ),

                Field.Set(this, "recruits", IsNullable.Yes, new Join(
                    ()=>this.GetField("name"),
                    ()=>this.GetField("distributorName"))
                ),
                Field.Set(OrderEntity.Instance, "orders", IsNullable.Yes, new Join(
                    ()=>this.GetField("name"),
                    ()=>OrderEntity.Instance.GetField("sellerName"))
                ),
                Field.Set(SellerBadgeEntity.Instance, "sellerBadges", IsNullable.No, new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerBadgeEntity.Instance.GetField("sellerName"))
                ),
                Field.Set(SellerProductTotalEntity.Instance, "sellerProductTotals", IsNullable.Yes, new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerBadgeEntity.Instance.GetField("sellerName"))
                ),
                Field.CalculatedSet(this, "descendants", IsNullable.Yes,
                    (tableAlias) => $"SELECT s.* FROM tvf_AllDescendants({tableAlias}.Name) d INNER JOIN Seller s ON d.Name = s.Name"
                ),
                Field.CalculatedSet(this, "ancestors", IsNullable.Yes,
                    (tableAlias) => $"SELECT s.* FROM tvf_AllAncestors({tableAlias}.Name) a INNER JOIN Seller s ON a.Name = s.Name"
                )
            };
        }
    }
}
