using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace GraphqlToTsql.Introspection
{
    internal static class IntrospectionEntityList
    {
        public static List<EntityBase> All()
        {
            return new List<EntityBase>
            {
                GqlDirectiveDef.Instance,
                GqlEnumValueDef.Instance,
                GqlFieldDef.Instance,
                GqlInputValueDef.Instance,
                GqlSchemaDef.Instance,
                GqlTypeDef.Instance
            };
        }
    }
}
