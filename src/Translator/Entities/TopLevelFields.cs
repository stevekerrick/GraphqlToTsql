﻿using System.Collections.Generic;

namespace GraphqlToTsql.Translator.Entities
{
    //todo: eliminate and use AllEntities instead
    public static class TopLevelFields
    {
        public static List<Field> All = new List<Field>
        {
            Field.Set(EpcDef.Instance, "epcs", null)
        };
    }
}