namespace GraphqlToSql.Transpiler.Entities
{
    public class Field
    {
        public EntityBase Entity { get; private set; }
        public string Name { get; private set; }
        public FieldType FieldType { get; private set; }
        public string DbColumnName { get; private set; }
        //public Func<string> JoinFunc { get; }

        private Field() { }

        public static Field Scalar(EntityBase entity, string name, string dbColumnName) => new Field
        {
            FieldType = FieldType.Scalar,
            Entity = entity,
            Name = name,
            DbColumnName = dbColumnName
        };

        public static Field Row(EntityBase entity, string name) => new Field
        {
            FieldType = FieldType.Row,
            Entity = entity,
            Name = name
        };

        public static Field Set(EntityBase entity, string name) => new Field
        {
            FieldType = FieldType.Set,
            Entity = entity,
            Name = name
        };
    }

    public enum FieldType
    {
        Scalar,
        Row,
        Set
    }
}
