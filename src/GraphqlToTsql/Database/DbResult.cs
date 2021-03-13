namespace GraphqlToTsql.Database
{
    internal class DbResult
    {
        public string DataJson { get; set; }
        public string DbError { get; set; }
        public long? DatabaseQueryTime { get; set; }
    }
}
