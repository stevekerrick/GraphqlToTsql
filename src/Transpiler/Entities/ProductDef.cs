using System.Collections.Generic;

namespace GraphqlToSql.Transpiler.Entities
{
    public class ProductDef : EntityBase
    {
        public static ProductDef Instance = new ProductDef();

        public override string Name => "product";
        public override string DbTableName => "Product";

        private ProductDef()
        {
            Fields = new List<Field>
            {
                Field.Scalar(this, "id", "Id"),
                Field.Scalar(this, "urn", "Urn"),
                Field.Scalar(this, "name", "Name")
            };
        }
    }
}
