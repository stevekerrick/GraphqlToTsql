using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace DemoEntities
{
    public class OrderEntity : EntityBase
    {
        public static OrderEntity Instance = new OrderEntity();

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


                Field.CalculatedField(this, "totalQuantity", ValueType.Int, IsNullable.No,
                    (tableAlias) => $"SELECT SUM(od.Quantity) FROM OrderDetail od WHERE {tableAlias}.Id = od.OrderId"
                ),
                Field.CalculatedField(this, "formattedDate", ValueType.String, IsNullable.No,
                    (tableAlias) => $"FORMAT({tableAlias}.[Date], 'dd/MM/yyyy', 'en-US' )"
                ),



                Field.Row(SellerEntity.Instance, "seller", new Join(
                    ()=>this.GetField("sellerName"),
                    ()=>SellerEntity.Instance.GetField("name"))
                ),

                Field.Set(OrderDetailEntity.Instance, "orderDetails", new Join(
                    ()=>this.GetField("id"),
                    ()=>OrderDetailEntity.Instance.GetField("orderId"))
                )
            };
        }
    }
}
