using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace DemoEntities
{
    public class SellerBadgeEntity : EntityBase
    {
        public static SellerBadgeEntity Instance = new SellerBadgeEntity();

        public override string Name => "sellerBadge";
        public override string DbTableName => "SellerBadge";
        public override string[] PrimaryKeyFieldNames => new[] { "sellerName", "badgeName" };

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "sellerName", "SellerName", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "badgeName", "BadgeName", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "dateAwarded", "DateAwarded", ValueType.String, IsNullable.No),

                Field.Row(SellerEntity.Instance, "seller", new Join(
                    ()=>this.GetField("sellerName"),
                    ()=>SellerEntity.Instance.GetField("name"))
                ),
                Field.Row(BadgeEntity.Instance, "badge", new Join(
                    ()=>this.GetField("badgeName"),
                    ()=>BadgeEntity.Instance.GetField("name"))
                )
            };
        }
    }
}
