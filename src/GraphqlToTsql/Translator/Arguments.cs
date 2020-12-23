using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    public class Arguments
    {
        public long? Offset { get; set; }
        public long? First { get; set; }
        public List<Filter> Filters { get; set; }

        public Arguments()
        {
            Filters = new List<Filter>();
        }

        public void Add(Field field, string name, Value value)
        {
            if (name == "offset")
            {
                Offset = IntValue(name, value);
            }
            else if (name == "first")
            {
                First = IntValue(name, value);
            }
            else
            {
                var argumentField = field.Entity.GetField(name);
                Filters.Add(new Filter(argumentField, value));
            }
        }

        private long IntValue(string name, Value value)
        {
            if (value.ValueType != ValueType.Number)
            {
                throw new Exception($"{name} must be an integer: {value.RawValue}");
            }

            return (long)(decimal)value.RawValue;
        }

        public class Filter
        {
            public Field Field { get; }
            public Value Value { get; }

            public Filter(Field field, Value value)
            {
                Field = field;
                Value = value;
            }
        }
    }
}
