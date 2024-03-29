﻿using GraphqlToTsql.Entities;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace DemoEntities
{
    public class SellerProductTotalEntity : EntityBase
    {
        public static SellerProductTotalEntity Instance = new SellerProductTotalEntity();

        public override string Name => "sellerProductTotal";
        public override string DbTableName => "SellerProductTotal";
        public override string[] PrimaryKeyFieldNames => new[] { "sellerName", "productName" };
        public override string SqlDefinition => @"
SELECT
  o.SellerName
, od.ProductName
, SUM(od.Quantity) AS TotalQuantity
, SUM(od.Quantity * p.Price) AS TotalAmount
FROM OrderDetail od
INNER JOIN [Order] o
  ON od.OrderId = o.Id
INNER JOIN Product p
  ON od.ProductName = p.[Name]
GROUP BY o.SellerName, od.ProductName
".Trim();

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "sellerName", "SellerName", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "productName", "ProductName", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "totalQuantity", "TotalQuantity", ValueType.Int, IsNullable.No),
                Field.Column(this, "totalAmount", "TotalAmount", ValueType.Float, IsNullable.No),

                Field.Row(SellerEntity.Instance, "seller", new Join(
                    ()=>this.GetField("sellerName"),
                    ()=>SellerEntity.Instance.GetField("name"))
                ),
                Field.Row(ProductEntity.Instance, "product", new Join(
                    ()=>this.GetField("productName"),
                    ()=>ProductEntity.Instance.GetField("name"))
                ),
            };
        }
    }
}
