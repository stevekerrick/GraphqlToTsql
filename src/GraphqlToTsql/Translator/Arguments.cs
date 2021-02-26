using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    public class Arguments
    {
        public long? First { get; set; }
        public long? Offset { get; set; }
        public string After { get; set; }
        public bool If { get; set; }

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
                return;
            }

            if (name == Constants.OFFSET_ARGUMENT)
            {
                Offset = IntValue(name, value, context);
                if (Offset != null && After != null)
                {
                    throw new InvalidRequestException("You can't use 'offset' and 'after' at the same time", context);
                }
                return;
            }

            if (name == Constants.AFTER_ARGUMENT)
            {
                After = StringValue(name, value, context);
                if (Offset != null && After != null)
                {
                    throw new InvalidRequestException("You can't use 'offset' and 'after' at the same time", context);
                }
                return;
            }

            if (field.FieldType == FieldType.Directive && name == Constants.IF_ARGUMENT)
            {
                If = BoolValue(name, value, context);
                return;
            }

            if (field.FieldType == FieldType.Directive)
            {
                throw new InvalidRequestException($"Invalid directive argument: {name}", context);
            }

            var argumentField = field.Entity.GetField(name, context);
            var newValue = new Value(argumentField.ValueType, value, () => $"Argument is the wrong type: {field.Entity.EntityType}.{name} is type {argumentField.ValueType}");
            if (newValue.ValueType == ValueType.Null && argumentField.IsNullable == IsNullable.No)
            {
                throw new InvalidRequestException($"{field.Entity.EntityType}.{name} can not be null", context);
            }

            Filters.Add(new Filter(argumentField, newValue));
        }

        private long IntValue(string name, Value value, Context context)
        {
            if (value.ValueType != ValueType.Int)
            {
                throw new InvalidRequestException($"{name} must be an Int: {value.RawValue}", context);
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

        private bool BoolValue(string name, Value value, Context context)
        {
            if (value.ValueType != ValueType.Boolean)
            {
                throw new InvalidRequestException($"{name} must be a Boolean: {value.RawValue}", context);
            }

            return (bool)value.RawValue;
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
