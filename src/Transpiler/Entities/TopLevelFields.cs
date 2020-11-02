using System.Collections.Generic;

namespace GraphqlToSql.Transpiler.Entities
{
    public static class TopLevelFields
    {
        public static List<Field> All = new List<Field>
        {
            Field.Set(CodeDef.Instance, "codes")
        };
    }
}
