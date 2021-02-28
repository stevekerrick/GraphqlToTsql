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
        public override string[] PrimaryKeyFieldNames => new[] { "name" };

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),
                Field.Column(this, "price", "Price", ValueType.Float, IsNullable.No),

                Field.CalculatedField(this, "totalRevenue", ValueType.Float, IsNullable.No,
                    (tableAlias) => $"SELECT (SELECT SUM(od.Quantity) FROM OrderDetail od WHERE {tableAlias}.[Name] = od.ProductName) * {tableAlias}.Price"
                ),

                Field.Set(OrderDetailDef.Instance, "orderDetails", IsNullable.Yes, new Join(
                    ()=>this.GetField("name"),
                    ()=>OrderDetailDef.Instance.GetField("productName"))
                ),
                Field.Set(SellerProductTotalDef.Instance, "sellerProductTotals", IsNullable.Yes, new Join(
                    ()=>this.GetField("name"),
                    ()=>OrderDetailDef.Instance.GetField("productName"))
                )
            };
        }
    }
}
