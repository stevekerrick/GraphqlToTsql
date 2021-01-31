using System.Collections.Generic;

namespace GraphqlToTsql.Introspection
{
    internal class GqlField
    {
        public string name { get; set; }
        public string description { get; set; }
        public List<GqlInputValue> args { get; set; }
        public GqlType type { get; set; }
        public bool isDeprecated { get; set; }
        public string deprecationReason { get; set; }
    }
}
