using System.Collections.Generic;

namespace GraphqlToTsql.Translator.Entities
{
    public class EpcDef : EntityBase
    {
        public static EpcDef Instance = new EpcDef();
        public override string Name => "epc";
        public override string DbTableName => "Epc";

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Scalar(this, "id", "Id"),
                Field.Scalar(this, "urn", "Urn"),
                Field.Scalar(this, "dispositionId", "DispositionId"),
                Field.Scalar(this, "parentId", "ParentId"),
                Field.Scalar(this, "bizLocationId", "BizLocationId"),
                Field.Scalar(this, "readPointId", "ReadPointId"),
                Field.Scalar(this, "productId", "ProductId"),
                Field.Scalar(this, "lotId", "LotId"),
                Field.Scalar(this, "lastUpdate", "LastUpdate"),

                Field.CalculatedField(this, "dispositionName", (tableAlias) => $"SELECT d.Name FROM Disposition d WHERE d.Id = {tableAlias}.DispositionId"),

                Field.Row(this, "parent", new Join(
                    ()=>this.GetField("parentId"),
                    ()=>this.GetField("id"))
                ),
                Field.Row(DispositionDef.Instance, "disposition", new Join(
                    ()=>this.GetField("dispositionId"),
                    ()=>DispositionDef.Instance.GetField("id"))
                ),
                Field.Row(LocationDef.Instance, "bizLocation", new Join(
                    ()=>this.GetField("bizLocationId"),
                    ()=>LocationDef.Instance.GetField("id"))
                ),
                Field.Row(LocationDef.Instance, "readPoint", new Join(
                    ()=>this.GetField("readPointId"),
                    ()=>LocationDef.Instance.GetField("id"))
                ),
                Field.Row(ProductDef.Instance, "product", new Join(
                    ()=>this.GetField("productId"),
                    ()=>ProductDef.Instance.GetField("id"))
                ),
                Field.Row(LotDef.Instance, "lot", new Join(
                    ()=>this.GetField("lotId"),
                    ()=>LotDef.Instance.GetField("id"))
                ),

                Field.Set(this, "children", new Join(
                    ()=>this.GetField("id"),
                    ()=>this.GetField("parentId"))
                )
            };
        }
    }
}
