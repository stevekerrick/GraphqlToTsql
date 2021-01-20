//using GraphqlToTsql.Entities;
//using GraphqlToTsql.Translator;
//using System.Collections.Generic;

//namespace DemoEntities
//{
//    public class EpcDef : EntityBase
//    {
//        public static EpcDef Instance = new EpcDef();
//        public override string Name => "epc";
//        public override string DbTableName => "Epc";
//        public override string PrimaryKeyFieldName => "id";

//        protected override List<Field> BuildFieldList()
//        {
//            return new List<Field>
//            {
//                Field.Scalar(this, "id", "Id", ValueType.Number),
//                Field.Scalar(this, "urn", "Urn", ValueType.String),
//                Field.Scalar(this, "dispositionUrn", "DispositionUrn", ValueType.String),
//                Field.Scalar(this, "parentId", "ParentId", ValueType.Number),
//                Field.Scalar(this, "bizLocationId", "BizLocationId", ValueType.Number),
//                Field.Scalar(this, "readPointId", "ReadPointId", ValueType.Number),
//                Field.Scalar(this, "productId", "ProductId", ValueType.Number),
//                Field.Scalar(this, "lotNumber", "LotNumber", ValueType.String),
//                Field.Scalar(this, "lastUpdate", "LastUpdate", ValueType.String),

//                Field.CalculatedField(this, "dispositionName", 
//                    (tableAlias) => $"SELECT d.Name FROM Disposition d WHERE d.Urn = {tableAlias}.DispositionUrn"
//                ),

//                Field.Row(this, "parent", new Join(
//                    ()=>this.GetField("parentId"),
//                    ()=>this.GetField("id"))
//                ),
//                Field.Row(DispositionDef.Instance, "disposition", new Join(
//                    ()=>this.GetField("dispositionUrn"),
//                    ()=>DispositionDef.Instance.GetField("urn"))
//                ),
//                Field.Row(LocationDef.Instance, "bizLocation", new Join(
//                    ()=>this.GetField("bizLocationId"),
//                    ()=>LocationDef.Instance.GetField("id"))
//                ),
//                Field.Row(LocationDef.Instance, "readPoint", new Join(
//                    ()=>this.GetField("readPointId"),
//                    ()=>LocationDef.Instance.GetField("id"))
//                ),
//                Field.Row(ProductDef.Instance, "product", new Join(
//                    ()=>this.GetField("productId"),
//                    ()=>ProductDef.Instance.GetField("id"))
//                ),
//                Field.Row(LotDef.Instance, "lot", new Join(
//                    ()=>this.GetField("lotNumber"),
//                    ()=>LotDef.Instance.GetField("lotNumber"))
//                ),

//                Field.Set(this, "children", new Join(
//                    ()=>this.GetField("id"),
//                    ()=>this.GetField("parentId"))
//                ),

//                Field.CalculatedSet(this, "descendants",
//                    (tableAlias) => $"SELECT e.* FROM tvf_AllDescendants({tableAlias}.Id) d INNER JOIN Epc e ON d.Id = e.Id"
//                ),
//                Field.CalculatedSet(this, "ancestors",
//                    (tableAlias) => $"SELECT e.* FROM tvf_AllAncestors({tableAlias}.Id) d INNER JOIN Epc e ON d.Id = e.Id"
//                )
//            };
//        }
//    }
//}
