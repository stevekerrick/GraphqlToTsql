using System.Collections.Generic;

namespace GraphqlToTsql.Translator.Entities
{
    public static class TopLevelFields
    {
        public static List<Field> All = new List<Field>
        {
            Field.Set(EpcDef.Instance, "epcs")
        };
    }
}
