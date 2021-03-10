using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    public class TsqlResult
    {
        public string Tsql { get; set; }
        public Dictionary<string, object> TsqlParameters { get; set; }
        public string Error { get; set; }
    }
}
