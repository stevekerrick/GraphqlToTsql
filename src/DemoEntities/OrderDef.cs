using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace DemoEntities
{
    public class OrderDef : EntityBase
    {
        public static OrderDef Instance = new OrderDef();

        public override string Name => "order";
        public override string DbTableName => "Order";
        public override string[] PrimaryKeyFieldNames => new[] { "id" };
        public override long? MaxPageSize => 1000L;

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "id", "Id", ValueType.Int, IsNullable.No),
                Field.Column(this, "sellerName", "SellerName", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "date", "Date", ValueType.String, IsNullable.No),
                Field.Column(this, "shipping", "Shipping", ValueType.Float, IsNullable.No),

                Field.Row(SellerDef.Instance, "seller", new Join(
                    ()=>this.GetField("sellerName"),
                    ()=>SellerDef.Instance.GetField("name"))
                ),

                Field.Set(OrderDetailDef.Instance, "orderDetails", IsNullable.No, new Join(
                    ()=>this.GetField("id"),
                    ()=>OrderDetailDef.Instance.GetField("orderId"))
                )
            };
        }
    }
}
