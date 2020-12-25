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

        public void Add(Field field, string name, Value value, Context context = null)
        {
            if (name == "offset")
            {
                Offset = IntValue(name, value, context);
            }
            else if (name == "first")
            {
                First = IntValue(name, value, context);
            }
            else
            {
                var argumentField = field.Entity.GetField(name, context);
                Filters.Add(new Filter(argumentField, value));
            }
        }

        private long IntValue(string name, Value value, Context context)
        {
            if (value.ValueType != ValueType.Number)
            {
                throw new InvalidRequestException($"{name} must be an integer: {value.RawValue}", context);
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
