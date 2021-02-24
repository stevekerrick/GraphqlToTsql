namespace GraphqlToTsql.Introspection
{
    internal class GqlEnumValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeprecated { get; set; }
        public string DeprecationReason { get; set; }

        public GqlEnumValue(string name)
        {
            Name = name;
            Description = "";
        }
    }
}
