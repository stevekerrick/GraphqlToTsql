﻿using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Translator.ValueType;

namespace DemoEntities
{
    public class SellerProductTotalDef : EntityBase
    {
        public static SellerProductTotalDef Instance = new SellerProductTotalDef();

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
                Field.Column(this, "sellerName", "SellerName", ValueType.String),
                Field.Column(this, "productName", "ProductName", ValueType.String),
                Field.Column(this, "totalQuantity", "TotalQuantity", ValueType.Number),
                Field.Column(this, "totalAmount", "TotalAmount", ValueType.Number),

                Field.Row(SellerDef.Instance, "seller", new Join(
                    ()=>this.GetField("sellerName"),
                    ()=>SellerDef.Instance.GetField("name"))
                ),
                Field.Row(ProductDef.Instance, "product", new Join(
                    ()=>this.GetField("productName"),
                    ()=>ProductDef.Instance.GetField("name"))
                ),
            };
        }
    }
}
