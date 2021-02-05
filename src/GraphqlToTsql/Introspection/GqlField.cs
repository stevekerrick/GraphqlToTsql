//using System.Collections.Generic;

//namespace GraphqlToTsql.Introspection
//{
//    internal class GqlField
//    {
//        public string name { get; set; }
//        public string description { get; set; }
//        public List<GqlInputValue> args { get; set; }
//        public GqlType type { get; set; }
//        public bool isDeprecated { get; set; }
//        public string deprecationReason { get; set; }

//        public GqlField(string name, GqlType type, bool includedDeprecated = false)
//        {
//            this.name = name;
//            this.type = type;
//            args = new List<GqlInputValue>();

//            if (includedDeprecated)
//            {
//                args.Add(new GqlInputValue { 
//                    Name = "includeDeprecated",
//                    Type = GqlType.Scalar("Boolean", ""),
//                    DefaultValue = "false"
//                });
//            }
//        }
//    }
//}
