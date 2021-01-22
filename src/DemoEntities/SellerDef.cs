using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;

namespace DemoEntities
{
    public class SellerDef : EntityBase
    {
        public static SellerDef Instance = new SellerDef();

        public override string Name => "seller";
        public override string DbTableName => "Seller";
        public override string PrimaryKeyFieldName => "name";

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Scalar(this, "name", "Name", ValueType.String),
                Field.Scalar(this, "distributorName", "DistributorName", ValueType.String),
                Field.Scalar(this, "city", "City", ValueType.String),
                Field.Scalar(this, "state", "State", ValueType.String),
                Field.Scalar(this, "postalCode", "PostalCode", ValueType.String),

                Field.Row(this, "distributor", new Join(
                    ()=>this.GetField("distributorName"),
                    ()=>Instance.GetField("name"))
                ),

                Field.Set(this, "recruits", new Join(
                    ()=>this.GetField("name"),
                    ()=>this.GetField("distributorName"))
                ),
                Field.Set(OrderDef.Instance, "orders", new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerBadgeDef.Instance.GetField("sellerName"))
                ),
                Field.Set(SellerBadgeDef.Instance, "sellerBadges", new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerBadgeDef.Instance.GetField("sellerName"))
                )
            };
        }
    }
}
