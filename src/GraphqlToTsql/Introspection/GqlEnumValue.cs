namespace GraphqlToTsql.Introspection
{
    internal class GqlEnumValue
    {
        public string name { get; set; }
        public string description { get; set; }
        public bool isDeprecated { get; set; }
        public string deprecationReason { get; set; }

        public GqlEnumValue(string name)
        {
            this.name = name;
            this.description = "";
        }
    }
}
