using System.Collections.Generic;
using System.Linq;

namespace GraphqlToTsql.Introspection
{
    internal class GqlType
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TypeKind Kind { get; set; }

        // OBJECT and INTERFACE only
        public List<GqlField> Fields { get; set; }

        // OBJECT only
        public List<GqlType> Interfaces { get; set; }

        // INTERFACE and UNION only
        public List<GqlType> PossibleTypes { get; set; }

        // ENUM only
        public List<GqlEnumValue> EnumValues { get; set; }

        // INPUT_OBJECT only
        public List<GqlInputValue> InputFields { get; set; }

        // NON_NULL and LIST only
        public GqlType OfType { get; set; }

        public static GqlType Scalar(string name, string description)
        {
            return new GqlType
            {
                Key = name,
                Name = name,
                Description = description,
                Kind = TypeKind.SCALAR
            };
        }

        public static GqlType NonNullable(GqlType baseType)
        {
            return new GqlType
            {
                Key = $"{TypeKind.NON_NULL}:{baseType.Key}",
                Name = null,
                Kind = TypeKind.NON_NULL,
                OfType = baseType
            };
        }

        public static GqlType Object(string name)
        {
            return new GqlType
            {
                Key = name,
                Name = name,
                Kind = TypeKind.OBJECT,
                Fields = new List<GqlField>(),
                Interfaces = new List<GqlType>()
            };
        }

        public static GqlType List(GqlType baseType)
        {
            return new GqlType
            {
                Key = $"{TypeKind.LIST}:{baseType.Key}",
                Name = null,
                Kind = TypeKind.LIST,
                OfType = baseType
            };
        }

        public static GqlType Enum(string name, params string[] values)
        {
            var enumValues = values
                .Select(v => new GqlEnumValue(v))
                .ToList();

            return new GqlType
            {
                Key = name,
                Name = name,
                Kind = TypeKind.ENUM,
                Fields = new List<GqlField>(),
                Interfaces = new List<GqlType>(),
                EnumValues = enumValues
            };
        }
 
        public static GqlType InputObject(string name)
        {
            return new GqlType
            {
                Key = name,
                Name = name,
                Kind = TypeKind.INPUT_OBJECT,
                InputFields = new List<GqlInputValue>()
            };
        }
    }

    internal enum TypeKind
    {
        SCALAR,
        OBJECT,
        INTERFACE,
        UNION,
        ENUM,
        INPUT_OBJECT,
        LIST,
        NON_NULL
    }
}
