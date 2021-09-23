using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GraphqlToTsql.Translator
{
    internal class OrderBy
    {
        public List<OrderByField> Fields { get; private set; }

        public OrderBy()
        {
            Fields = new List<OrderByField>();
        }

        public void Add(Field field, Direction direction)
        {
            Fields.Add(new OrderByField { Field = field, Direction = direction });
        }
    }

    internal class OrderByField
    {
        public Field Field { get; set; }
        public Direction Direction { get; set; }
    }

    internal enum Direction
    {
        asc,
        desc
    }
}
