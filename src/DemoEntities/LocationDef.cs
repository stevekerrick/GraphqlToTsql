//using GraphqlToTsql.Entities;
//using GraphqlToTsql.Translator;
//using System.Collections.Generic;

//namespace DemoEntities
//{
//    public class LocationDef : EntityBase
//    {
//        public static LocationDef Instance = new LocationDef();

//        public override string Name => "location";
//        public override string DbTableName => "Location";
//        public override string PrimaryKeyFieldName => "id";

//        protected override List<Field> BuildFieldList()
//        {
//            return new List<Field>
//            {
//                Field.Scalar(this, "id", "Id", ValueType.Number),
//                Field.Scalar(this, "urn", "Urn", ValueType.String),
//                Field.Scalar(this, "name", "Name", ValueType.String),
//                Field.Scalar(this, "isActive", "IsActive", ValueType.Boolean),

//                Field.Set(EpcDef.Instance, "epcs", new Join(
//                    ()=>this.GetField("id"),
//                    ()=>EpcDef.Instance.GetField("bizLocationId"))
//                ),
//                Field.Set(LotDef.Instance, "lots", new Join(
//                    ()=>this.GetField("id"),
//                    ()=>LotDef.Instance.GetField("locationId"))
//                )
//            };
//        }
//    }
//}
