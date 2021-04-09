using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace DemoEntities
{
    public class OrderDetailEntity : EntityBase
    {
        public static OrderDetailEntity Instance = new OrderDetailEntity();

        public override string Name => "orderDetail";
        public override string DbTableName => "OrderDetail";
        public override string[] PrimaryKeyFieldNames => new[] { "orderId", "productName" };

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "orderId", "OrderId", ValueType.Int, IsNullable.No),
                Field.Column(this, "productName", "ProductName", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "quantity", "Quantity", ValueType.Int, IsNullable.No),

                Field.Row(OrderEntity.Instance, "order", new Join(
                    ()=>this.GetField("orderId"),
                    ()=>OrderEntity.Instance.GetField("id"))
                ),
                Field.Row(ProductEntity.Instance, "product", new Join(
                    ()=>this.GetField("productName"),
                    ()=>ProductEntity.Instance.GetField("name"))
                )
            };
        }
    }
}
