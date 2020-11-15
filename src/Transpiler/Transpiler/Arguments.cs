using System;
using System.Collections.Generic;
using System.Linq;
using GraphqlToSql.Transpiler.Entities;

namespace GraphqlToSql.Transpiler.Transpiler
{
    public class Arguments
    {
        public long? Offset { get; set; }
        public long? Limit { get; set; }
        public List<JoinColumn> JoinColumns { get; set; }

        public Arguments() { 
            JoinColumns = new List<JoinColumn>();
        }

        public void Add(Field field, string name, Value value)
        {
            if (name == "offset")
            {
                Offset = IntValue(name, value);
            }
            else if (name == "limit")
            {
                Limit = IntValue(name, value);
            }
            else
            {
                var argumentField = field.Entity.Fields.FirstOrDefault(_ => _.Name == name);
                if (argumentField == null)
                {
                    throw new Exception($"Unknown field: {name}");
                }
                JoinColumns.Add(new JoinColumn(argumentField, value));
            }
        }

        private long IntValue(string name, Value value)
        {
            if (value.ValueType != ValueType.Number)
            {
                throw new Exception($"{name} must be an integer: {value.ValueString}");
            }

            var doubleValue = Double.Parse(value.ValueString);
            return (long)doubleValue;
        }

    }

    public class JoinColumn
    {
        public Field Field { get; }
        public Value Value { get; }

        public JoinColumn(Field field, Value value)
        {
            Field = field;
            Value = value;
        }
    }
}
