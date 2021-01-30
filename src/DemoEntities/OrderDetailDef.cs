using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;

namespace DemoEntities
{
    public class OrderDetailDef : EntityBase
    {
        public static OrderDetailDef Instance = new OrderDetailDef();

        public override string Name => "orderDetail";
        public override string DbTableName => "OrderDetail";
        public override string[] PrimaryKeyFieldNames => new[] { "orderId", "productName" };

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "orderId", "OrderId", ValueType.Int),
                Field.Column(this, "productName", "ProductName", ValueType.String),
                Field.Column(this, "quantity", "Quantity", ValueType.Int),

                Field.Row(OrderDef.Instance, "order", new Join(
                    ()=>this.GetField("orderId"),
                    ()=>OrderDef.Instance.GetField("id"))
                ),
                Field.Row(ProductDef.Instance, "product", new Join(
                    ()=>this.GetField("productName"),
                    ()=>ProductDef.Instance.GetField("name"))
                )
            };
        }
    }
}
