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
                GqlDirectiveEntity.Instance,
                GqlEnumValueEntity.Instance,
                GqlFieldEntity.Instance,
                GqlInputValueEntity.Instance,
                GqlSchemaEntity.Instance,
                GqlTypeEntity.Instance
            };
        }
    }
}
