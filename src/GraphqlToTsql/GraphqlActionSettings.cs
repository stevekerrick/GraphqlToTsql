using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace GraphqlToTsql
{
    public class GraphqlActionSettings
    {
        public bool AllowIntrospection { get; set; }
        public string ConnectionString { get; set; }
        public EmptySetBehavior EmptySetBehavior { get; set; }
        public List<EntityBase> EntityList { get; set; }
    }
}
