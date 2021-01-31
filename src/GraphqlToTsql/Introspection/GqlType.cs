using System.Collections.Generic;

namespace GraphqlToTsql.Introspection
{
    internal class GqlType
    {
        public string name { get; set; }
        public string description { get; set; }
        public TypeKind kind { get; set; }

        // OBJECT and INTERFACE only
        public List<GqlField> fields(bool includeDeprecated = false) { return null; } //TODO

        // OBJECT only
        public List<GqlType> interfaces { get; set; }

        // INTERFACE and UNION only
        public List<GqlType> possibleTypes { get; set; }

        // ENUM only
        public List<GqlEnumValue> enumValues(bool includeDeprecated = false) { return null; } //TODO

        // INPUT_OBJECT only
        public List<GqlInputValue> inputFields { get; set; }

        // NON_NULL and LIST only
        public GqlType ofType { get; set; }
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
