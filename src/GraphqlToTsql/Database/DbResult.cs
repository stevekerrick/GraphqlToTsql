using System.Collections.Generic;

namespace GraphqlToTsql.Database
{
    public class DbResult
    {
        public string DataJson { get; set; }
        public string DbError { get; set; }
        public List<Statistic> Statistics { get; set; }
    }
}
