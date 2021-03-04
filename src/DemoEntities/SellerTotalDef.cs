using GraphqlToTsql.Entities;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace DemoEntities
{
    public class SellerTotalDef : EntityBase
    {
        public static SellerTotalDef Instance = new SellerTotalDef();

        public override string Name => "sellerTotal";
        public override string DbTableName => "SellerTotal";
        public override string[] PrimaryKeyFieldNames => new[] { "sellerName" };
        public override string SqlDefinition => @"
SELECT
  s.[Name] AS SellerName
, COUNT(DISTINCT o.Id) AS TotalOrders
, SUM(od.Quantity) AS TotalQuantity
, SUM(od.Quantity * p.Price) AS TotalAmount
FROM Seller s
INNER JOIN [Order] o
  ON s.Name = o.SellerName
INNER JOIN OrderDetail od
  ON o.Id = od.OrderId
INNER JOIN Product p
  ON od.ProductName = p.[Name]
GROUP BY s.[Name]
".Trim();

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "sellerName", "SellerName", ValueType.String, IsNullable.No),
                Field.Column(this, "totalOrders", "TotalOrders", ValueType.Int, IsNullable.No),
                Field.Column(this, "totalQuantity", "TotalQuantity", ValueType.Int, IsNullable.No),
                Field.Column(this, "totalAmount", "TotalAmount", ValueType.Float, IsNullable.No),

                Field.Row(SellerDef.Instance, "seller", new Join(
                    ()=>this.GetField("sellerName"),
                    ()=>SellerDef.Instance.GetField("name"))
                )
            };
        }
    }
}
