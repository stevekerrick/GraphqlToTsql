using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;

namespace DemoEntities
{
    public class SellerDef : EntityBase
    {
        public static SellerDef Instance = new SellerDef();

        public override string Name => "seller";
        public override string DbTableName => "Seller";
        public override string[] PrimaryKeyFieldNames => new[] { "name" };

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "name", "Name", ValueType.String),
                Field.Column(this, "distributorName", "DistributorName", ValueType.String),
                Field.Column(this, "city", "City", ValueType.String),
                Field.Column(this, "state", "State", ValueType.String),
                Field.Column(this, "postalCode", "PostalCode", ValueType.String),

                Field.Row(this, "distributor", new Join(
                    ()=>this.GetField("distributorName"),
                    ()=>Instance.GetField("name"))
                ),
                Field.Row(SellerTotalDef.Instance, "sellerTotal", new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerTotalDef.Instance.GetField("sellerName"))
                ),
                Field.CalculatedRow(this, "apexDistributor",
                    (tableAlias) => $"SELECT s.* FROM tvf_AllAncestors({tableAlias}.Name) d INNER JOIN Seller s ON d.Name = s.Name AND s.DistributorName IS NULL"
                ),

                Field.Set(this, "recruits", new Join(
                    ()=>this.GetField("name"),
                    ()=>this.GetField("distributorName"))
                ),
                Field.Set(OrderDef.Instance, "orders", new Join(
                    ()=>this.GetField("name"),
                    ()=>OrderDef.Instance.GetField("sellerName"))
                ),
                Field.Set(SellerBadgeDef.Instance, "sellerBadges", new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerBadgeDef.Instance.GetField("sellerName"))
                ),
                Field.Set(SellerProductTotalDef.Instance, "sellerProductTotals", new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerBadgeDef.Instance.GetField("sellerName"))
                ),
                Field.CalculatedSet(this, "descendants",
                    (tableAlias) => $"SELECT s.* FROM tvf_AllDescendants({tableAlias}.Name) d INNER JOIN Seller s ON d.Name = s.Name"
                ),
                Field.CalculatedSet(this, "ancestors",
                    (tableAlias) => $"SELECT s.* FROM tvf_AllAncestors({tableAlias}.Name) a INNER JOIN Seller s ON a.Name = s.Name"
                )
            };
        }
    }
}
