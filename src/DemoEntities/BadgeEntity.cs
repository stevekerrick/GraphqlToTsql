using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace DemoEntities
{
    public class BadgeEntity : EntityBase
    {
        public static BadgeEntity Instance = new BadgeEntity();

        public override string Name => "badge";
        public override string DbTableName => "Badge";
        public override string[] PrimaryKeyFieldNames => new[] { "name" };

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "isSpecial", "IsSpecial", ValueType.Boolean, IsNullable.No),

                Field.Set(SellerBadgeEntity.Instance, "sellerBadges", IsNullable.Yes, new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerBadgeEntity.Instance.GetField("badgeName"))
                )
            };
        }
    }
}
