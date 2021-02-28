using System.Collections.Generic;

namespace GraphqlToTsql.Introspection
{
    internal class GqlDirective
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DirectiveLocation> Locations { get; set; }
        public List<GqlInputValue> Args { get; set; }
    }

    internal enum DirectiveLocation
    {
        QUERY,
        MUTATION,
        SUBSCRIPTION,
        FIELD,
        FRAGMENT_DEFINITION,
        FRAGMENT_SPREAD,
        INLINE_FRAGMENT,
        SCHEMA,
        SCALAR,
        OBJECT,
        FIELD_DEFINITION,
        ARGUMENT_DEFINITION,
        INTERFACE,
        UNION,
        ENUM,
        ENUM_VALUE,
        INPUT_OBJECT,
        INPUT_FIELD_DEFINITION,
    }
}
