using System.Collections.Generic;

namespace DemoEntities
{
    //todo: eliminate and use AllEntities instead
    public static class TopLevelFields
    {
        public static List<Field> All = new List<Field>
        {
            Field.Set(EpcDef.Instance, "epcs", null),
            Field.Row(ProductDef.Instance, "product", null),
            Field.Set(ProductDef.Instance, "products", null)
        };
    }
}
