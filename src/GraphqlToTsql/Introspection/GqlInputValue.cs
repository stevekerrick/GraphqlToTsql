namespace GraphqlToTsql.Introspection
{
    internal class GqlInputValue
    {
        public string name { get; set; }
        public string description { get; set; }
        public GqlType type { get; set; }
        public string defaultValue { get; set; }
    }
}
