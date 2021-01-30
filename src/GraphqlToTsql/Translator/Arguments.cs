using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    public class Arguments
    {
        public long? First { get; set; }
        public long? Offset { get; set; }
        public string After { get; set; }
        public List<Filter> Filters { get; set; }

        public Arguments()
        {
            Filters = new List<Filter>();
        }

        public void Add(Field field, string name, Value value, Context context)
        {
            if (name == Constants.FIRST_ARGUMENT)
            {
                First = IntValue(name, value, context);
            }
            else if (name == Constants.OFFSET_ARGUMENT)
            {
                Offset = IntValue(name, value, context);
                if (Offset != null && After != null)
                {
                    throw new InvalidRequestException("You can't use 'offset' and 'after' at the same time", context);
                }
            }
            else if (name == Constants.AFTER_ARGUMENT)
            {
                After = StringValue(name, value, context);
                if (Offset != null && After != null)
                {
                    throw new InvalidRequestException("You can't use 'offset' and 'after' at the same time", context);
                }
            }
            else
            {
                var argumentField = field.Entity.GetField(name, context);
                var newValue = new Value(argumentField.ValueType, value, () => $"Argument is the wrong type: {field.Entity.EntityType}.{name} is type {argumentField.ValueType}");
                Filters.Add(new Filter(argumentField, newValue));
            }
        }

        private long IntValue(string name, Value value, Context context)
        {
            if (value.ValueType != ValueType.Int)
            {
                throw new InvalidRequestException($"{name} must be an integer: {value.RawValue}", context);
            }

            return (long)value.RawValue;
        }

        private string StringValue(string name, Value value, Context context)
        {
            if (value.ValueType != ValueType.String)
            {
                throw new InvalidRequestException($"{name} must be a string: {value.RawValue}", context);
            }

            return (string)value.RawValue;
        }

        public class Filter
        {
            public Field Field { get; }
            public Value Value { get; }

            public Filter(Field field, Value value)
            {
                var coercedValue = new Value(field.ValueType, value, () => $"Argument is the wrong type: {field.Entity.EntityType}.{field.Name} is type {field.ValueType}");
                Field = field;
                Value = coercedValue;
            }
        }
    }
}
