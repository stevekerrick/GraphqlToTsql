using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    /// <summary>
    /// Stores the result of a parsed GQL ObjectValue
    /// </summary>
    internal class ObjectValue
    {
        public List<ObjectField> ObjectFields { get; set; }

        public ObjectValue()
        {
            ObjectFields = new List<ObjectField>();
        }
    }

    internal class ObjectField
    {
        public string Name { get; set; }
        public Value Value { get; set; }
    }
}
