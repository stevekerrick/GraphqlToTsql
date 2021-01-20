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
        public override string PrimaryKeyFieldName => "name";

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Scalar(this, "name", "Name", ValueType.String),
                Field.Scalar(this, "description", "Description", ValueType.String),
                Field.Scalar(this, "price", "Price", ValueType.Number),

                Field.Set(OrderDetailDef.Instance, "orderDetails", new Join(
                    ()=>this.GetField("name"),
                    ()=>OrderDetailDef.Instance.GetField("productName"))
                )
            };
        }
    }
}
