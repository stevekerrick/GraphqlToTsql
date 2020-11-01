using System.Collections.Generic;

namespace GraphqlToSql.Transpiler.Models
{
    public static class TopLevelFields
    {
        public static List<FieldDef> All = new List<FieldDef>
        {
            new FieldDef(CodeDef.Instance, "codes", dbColumnName: null, isList: true)
        };
    }
}
