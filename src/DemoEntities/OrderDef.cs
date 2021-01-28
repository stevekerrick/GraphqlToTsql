using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
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
                Field.Column(this, "id", "Id", ValueType.Number),
                Field.Column(this, "sellerName", "SellerName", ValueType.String),
                Field.Column(this, "date", "Date", ValueType.String),
                Field.Column(this, "shipping", "Shipping", ValueType.Number),

                Field.Row(SellerDef.Instance, "seller", new Join(
                    ()=>this.GetField("sellerName"),
                    ()=>SellerDef.Instance.GetField("name"))
                ),

                Field.Set(OrderDetailDef.Instance, "orderDetails", new Join(
                    ()=>this.GetField("id"),
                    ()=>OrderDetailDef.Instance.GetField("orderId"))
                )
            };
        }
    }
}
