namespace GraphqlToTsql.Database
{
    public class Statistic
    {
        public string Name { get; set; }
        public long? Value { get; set; }

        public Statistic(string name, long? value)
        {
            Name = name;
            Value = value;
        }
    }
}
