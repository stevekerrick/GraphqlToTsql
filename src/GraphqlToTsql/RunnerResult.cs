using GraphqlToTsql.Database;
using System.Collections.Generic;

namespace GraphqlToTsql
{
    public class RunnerResult
    {
        public string Tsql { get; set; }
        public Dictionary<string, object> TsqlParameters { get; set; }
        public string DataJson { get; set; }
        public string ParseError { get; set; }
        public string DbError { get; set; }
        public bool IsSuccessful => string.IsNullOrWhiteSpace(ParseError) || string.IsNullOrWhiteSpace(DbError);
        public List<Statistic> Statistics { get; set; }
    }
}
