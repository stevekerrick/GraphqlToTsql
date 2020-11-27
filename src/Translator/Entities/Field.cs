using System;
using System.Diagnostics;

namespace GraphqlToTsql.Translator.Entities
{
    [DebuggerDisplay("{Name,nq}")]
    public class Field
    {
        public EntityBase Entity { get; private set; }
        public string Name { get; private set; }
        public FieldType FieldType { get; private set; }
        public string DbColumnName { get; private set; }
        public Join Join { get; private set; }
        public Func<string, string> TemplateFunc { get; private set; }

        private Field() { }

        public static Field Scalar(EntityBase entity, string name, string dbColumnName) => new Field
        {
            FieldType = FieldType.Scalar,
            Entity = entity,
            Name = name,
            DbColumnName = dbColumnName
        };

        public static Field CalculatedField(EntityBase entity, string name,
            Func<string, string> templateFunc) => new Field
            {
                FieldType = FieldType.Scalar,
                Entity = entity,
                Name = name,
                TemplateFunc = templateFunc
            };

        public static Field Row(EntityBase entity, string name, Join join) => new Field
        {
            FieldType = FieldType.Row,
            Entity = entity,
            Name = name,
            Join = join
        };

        public static Field Set(EntityBase entity, string name, Join join) => new Field
        {
            FieldType = FieldType.Set,
            Entity = entity,
            Name = name,
            Join = join
        };
    }

    public enum FieldType
    {
        Scalar,
        Row,
        Set
    }
}
