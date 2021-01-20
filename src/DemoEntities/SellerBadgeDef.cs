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
        public override string PrimaryKeyFieldName => "sellerName"; //TODO: composite key

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Scalar(this, "sellerName", "SellerName", ValueType.String),
                Field.Scalar(this, "badgeName", "BadgeName", ValueType.String),
                Field.Scalar(this, "dateAwarded", "DateAwarded", ValueType.String),

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
