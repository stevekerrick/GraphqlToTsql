using System.Collections.Generic;

namespace GraphqlToTsql.Translator.Entities
{
    public class LotDef : EntityBase
    {
        public static LotDef Instance = new LotDef();

        public override string Name => "lot";
        public override string DbTableName => "Lot";

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Scalar(this, "id", "Id"),
                Field.Scalar(this, "lotNumber", "LotNumber"),
                Field.Scalar(this, "expirationDate", "ExpirationDt"),
                Field.Scalar(this, "productId", "ProductId"),

                Field.Row(ProductDef.Instance, "product", new Join(
                    ()=>this.GetField("productId"),
                    ()=>ProductDef.Instance.GetField("id"))
                ),

                Field.Set(EpcDef.Instance, "epcs", new Join(
                    ()=>this.GetField("id"),
                    ()=>EpcDef.Instance.GetField("lotId"))
                )
            };
        }
    }
}
