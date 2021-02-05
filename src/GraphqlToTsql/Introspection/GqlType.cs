//using System.Collections.Generic;
//using System.Linq;

//namespace GraphqlToTsql.Introspection
//{
//    internal class GqlType
//    {
//        public string Name { get; set; }
//        public string Description { get; set; }
//        public TypeKind Kind { get; set; }

//        // OBJECT and INTERFACE only
//        public List<GqlField> Fields { get; set; }

//        // OBJECT only
//        public List<GqlType> Interfaces { get; set; }

//        // INTERFACE and UNION only
//        public List<GqlType> PossibleTypes { get; set; }

//        // ENUM only
//        public List<GqlEnumValue> EnumValues { get; set; }

//        // INPUT_OBJECT only
//        public List<GqlInputValue> InputFields { get; set; }

//        // NON_NULL and LIST only
//        public GqlType OfType { get; set; }

//        public static GqlType Scalar(string name, string description)
//        {
//            return new GqlType
//            {
//                Name = name,
//                Description = description,
//                Kind = TypeKind.SCALAR
//            };
//        }

//        public static GqlType NonNullable(GqlType baseType)
//        {
//            return new GqlType
//            {
//                Name = null,
//                Kind = TypeKind.NON_NULL,
//                OfType = baseType
//            };
//        }

//        public static GqlType Object(string name)
//        {
//            return new GqlType
//            {
//                Name = name,
//                Kind = TypeKind.OBJECT,
//                Fields = new List<GqlField>(),
//                Interfaces = new List<GqlType>()
//            };
//        }

//        public static GqlType List(GqlType baseType)
//        {
//            return new GqlType
//            {
//                Name = null,
//                Kind = TypeKind.LIST,
//                OfType = baseType
//            };
//        }

//        public static GqlType Enum(string name, params string[] values)
//        {
//            var enumValues = values
//                .Select(v => new GqlEnumValue(v))
//                .ToList();

//            return new GqlType
//            {
//                Name = name,
//                Kind = TypeKind.ENUM,
//                Fields = new List<GqlField>(),
//                Interfaces = new List<GqlType>(),
//                EnumValues = enumValues
//            };
//        }
//    }

//    internal enum TypeKind
//    {
//        SCALAR,
//        OBJECT,
//        INTERFACE,
//        UNION,
//        ENUM,
//        INPUT_OBJECT,
//        LIST,
//        NON_NULL
//    }
//}
