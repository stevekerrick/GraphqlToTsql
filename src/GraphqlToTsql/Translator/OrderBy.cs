using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    internal class OrderByExp
    {
        public string FieldName { get; set; }
        public OrderByEnum OrderByEnum { get; set; }
    }

    internal class OrderBy
    {
        public List<OrderByField> Fields { get; private set; }

        public OrderBy()
        {
            Fields = new List<OrderByField>();
        }

        public void Add(Field field, OrderByEnum orderByEnum)
        {
            Fields.Add(new OrderByField { Field = field, OrderByEnum = orderByEnum });
        }
    }

    internal class OrderByField
    {
        public Field Field { get; set; }
        public OrderByEnum OrderByEnum { get; set; }
    }

    internal enum OrderByEnum
    {
        asc,
        desc
    }
}
