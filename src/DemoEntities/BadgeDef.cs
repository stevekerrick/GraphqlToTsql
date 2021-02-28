using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;

namespace DemoEntities
{
    public class BadgeDef : EntityBase
    {
        public static BadgeDef Instance = new BadgeDef();

        public override string Name => "badge";
        public override string DbTableName => "Badge";
        public override string[] PrimaryKeyFieldNames => new[] { "name" };

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "isSpecial", "IsSpecial", ValueType.Boolean, IsNullable.No),

                Field.Set(SellerBadgeDef.Instance, "sellerBadges", IsNullable.Yes, new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerBadgeDef.Instance.GetField("badgeName"))
                )
            };
        }
    }
}
