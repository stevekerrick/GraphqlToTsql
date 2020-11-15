using System.Collections.Generic;

namespace GraphqlToTsql.Translator.Entities
{
    public class EpcDef : EntityBase
    {
        public static EpcDef Instance = new EpcDef();

        public override string Name => "epc";
        public override string DbTableName => "Epc";

        private EpcDef()
        {
            Fields = new List<Field>
            {
                Field.Scalar(this, "id", "Id"),
                Field.Scalar(this, "urn", "Urn"),
                Field.Scalar(this, "parentId", "ParentId"),
                Field.Scalar(this, "dispositionId", "DispositionId"),
                Field.Scalar(this, "bizLocationId", "BizLocationId"),
                Field.Scalar(this, "readPointId", "ReadPointId"),
                Field.Scalar(this, "productId", "ProductId"),
                Field.Scalar(this, "lotId", "LotId"),
                Field.Scalar(this, "lastUpdate", "LastUpdate"),
                Field.Row(ProductDef.Instance, "product", new Join(
                    ()=>this.GetField("productId"),
                    ()=>ProductDef.Instance.GetField("id"))
                )
            };
        }
    }
}


