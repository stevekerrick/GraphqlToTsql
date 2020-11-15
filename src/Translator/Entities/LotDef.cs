using System.Collections.Generic;

namespace GraphqlToTsql.Translator.Entities
{
    public class LotDef : EntityBase
    {
        public static LotDef Instance = new LotDef();

        public override string Name => "lot";
        public override string DbTableName => "Lot";

        private LotDef()
        {
            Fields = new List<Field>
            {
                Field.Scalar(this, "id", "Id"),
                Field.Scalar(this, "productId", "ProductId"),
                Field.Scalar(this, "locationId", "LocationId"),
                Field.Scalar(this, "lotNumber", "LotNumber"),
                Field.Scalar(this, "expirationDate", "ExpirationDt"),

                Field.Row(ProductDef.Instance, "product", new Join(
                    ()=>this.GetField("productId"),
                    ()=>ProductDef.Instance.GetField("id"))
                ),
                Field.Row(LocationDef.Instance, "location", new Join(
                    ()=>this.GetField("locationId"),
                    ()=>LocationDef.Instance.GetField("id"))
                ),

                Field.Set(EpcDef.Instance, "epcs", new Join(
                    ()=>this.GetField("id"),
                    ()=>EpcDef.Instance.GetField("lotId"))
                )
            };
        }
    }
}
