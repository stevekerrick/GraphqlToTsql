using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace GraphqlToTsql.Introspection
{
    internal class GqlSchema
    {
        public List<GqlType> types { get; set; }
        public GqlType queryType { get; set; }
        public GqlType mutationType { get; set; }
        public GqlType subscriptionType { get; set; }
        public List<GqlDirective> directives { get; set; }

        public GqlSchema(List<EntityBase> entityList)
        {
        }
    }
}
