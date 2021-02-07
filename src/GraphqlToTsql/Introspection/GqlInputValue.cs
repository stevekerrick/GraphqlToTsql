namespace GraphqlToTsql.Introspection
{
    internal class GqlInputValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public GqlType Type { get; set; }
        public string DefaultValue { get; set; }
    }
}
