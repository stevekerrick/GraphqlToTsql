using System.Collections.Generic;

namespace GraphqlToTsql.Introspection
{
    internal class GqlField
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<GqlInputValue> Args { get; set; }
        public GqlType Type { get; set; }
        public bool IsDeprecated { get; set; }
        public string DeprecationReason { get; set; }

        public GqlField(string name, GqlType type)
        {
            this.Name = name;
            this.Type = type;

            Args = new List<GqlInputValue>();
            Args.Add(new GqlInputValue
            {
                Name = "includeDeprecated",
                Type = GqlType.Scalar("Boolean", ""),
                DefaultValue = "false"
            });
        }
    }
}
