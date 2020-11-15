using Newtonsoft.Json;
using System.Collections.Generic;

namespace GraphqlToTsql.Translator.Translator
{
    public class Query
    {
        public string Command { get; set; }
        public QueryParameters Parameters { get; set; }

        public class QueryParameters : Dictionary<string, object>
        {
            public override string ToString()
            {
                return $":params {JsonConvert.SerializeObject(this)}";
            }
        }
    }
}
