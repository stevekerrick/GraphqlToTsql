using System.Collections.Generic;
using Newtonsoft.Json;

namespace GraphqlToSql.Transpiler.Transpiler
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
