using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;

namespace DemoEntities
{
    public class ProductDef : EntityBase
    {
        public static ProductDef Instance = new ProductDef();

        public override string Name => "product";
        public override string DbTableName => "Product";
        public override string PrimaryKeyFieldName => "id";

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Scalar(this, "id", "Id", ValueType.Number),
                Field.Scalar(this, "urn", "Urn", ValueType.String),
                Field.Scalar(this, "name", "Name", ValueType.String),

                Field.Set(EpcDef.Instance, "epcs", new Join(
                    ()=>this.GetField("id"),
                    ()=>EpcDef.Instance.GetField("productId"))
                ),
                Field.Set(LotDef.Instance, "lots", new Join(
                    ()=>this.GetField("id"),
                    ()=>LotDef.Instance.GetField("productId"))
                )
            };
        }
    }
}
