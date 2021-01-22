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
        public override string PrimaryKeyFieldName => "id";

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Scalar(this, "id", "Id", ValueType.Number),
                Field.Scalar(this, "sellerName", "SellerName", ValueType.String),
                Field.Scalar(this, "date", "Date", ValueType.String),
                Field.Scalar(this, "shipping", "Shipping", ValueType.Number),
                Field.CalculatedField(this, "total", t1=>$"SELECT "), //TODO: calculated field here

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
