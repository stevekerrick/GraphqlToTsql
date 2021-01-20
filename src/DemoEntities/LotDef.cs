//using GraphqlToTsql.Entities;
//using GraphqlToTsql.Translator;
//using System.Collections.Generic;

//namespace DemoEntities
//{
//    public class LotDef : EntityBase
//    {
//        public static LotDef Instance = new LotDef();

//        public override string Name => "lot";
//        public override string DbTableName => "Lot";
//        public override string PrimaryKeyFieldName => "lotNumber";

//        protected override List<Field> BuildFieldList()
//        {
//            return new List<Field>
//            {
//                Field.Scalar(this, "lotNumber", "LotNumber", ValueType.String),
//                Field.Scalar(this, "expirationDate", "ExpirationDt", ValueType.String),
//                Field.Scalar(this, "productId", "ProductId", ValueType.Number),

//                Field.Row(ProductDef.Instance, "product", new Join(
//                    ()=>this.GetField("productId"),
//                    ()=>ProductDef.Instance.GetField("id"))
//                ),

//                Field.Set(EpcDef.Instance, "epcs", new Join(
//                    ()=>this.GetField("lotNumber"),
//                    ()=>EpcDef.Instance.GetField("lotNumber"))
//                )
//            };
//        }
//    }
//}
