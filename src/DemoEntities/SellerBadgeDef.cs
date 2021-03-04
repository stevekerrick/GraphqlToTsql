using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;

namespace DemoEntities
{
    public class SellerBadgeDef : EntityBase
    {
        public static SellerBadgeDef Instance = new SellerBadgeDef();

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

                Field.Row(SellerDef.Instance, "seller", new Join(
                    ()=>this.GetField("sellerName"),
                    ()=>SellerDef.Instance.GetField("name"))
                ),
                Field.Row(BadgeDef.Instance, "badge", new Join(
                    ()=>this.GetField("badgeName"),
                    ()=>BadgeDef.Instance.GetField("name"))
                )
            };
        }
    }
}
