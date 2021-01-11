using GraphqlToTsql.Util;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GraphqlToTsql.Entities
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
        public Func<string, string> MutatorFunc { get; private set; }

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

        public static Field CalculatedSet(EntityBase entity, string name,
            Func<string, string> templateFunc) => new Field
            {
                FieldType = FieldType.Set,
                Entity = entity,
                Name = name,
                TemplateFunc = templateFunc
            };

        public static Field Connection(Field setField) => new Field
        {
            FieldType = FieldType.Connection,
            Entity = new ConnectionEntity(setField),
            Name = $"{setField.Name}{Constants.CONNECTION}",
            Join = setField.Join
        };

        public static Field TotalCount(Field setField) => new Field
        {
            FieldType = FieldType.TotalCount,
            Entity = setField.Entity,
            Name = Constants.TOTAL_COUNT,
            Join = setField.Join
        };

        public static Field Edges(Field setField) => new Field
        {
            FieldType = FieldType.Edge,
            Entity = new EdgeEntity(setField),
            Name = Constants.EDGES,
            Join = setField.Join
        };

        public static Field Node(Field setField) => new Field
        {
            FieldType = FieldType.Node,
            Entity = new NodeEntity(setField),
            Name = Constants.NODE
        };

        public static Field Cursor(Field setField)
        {
            var entity = setField.Entity;

            return new Field
            {
                FieldType = FieldType.Cursor,
                Entity = new NodeEntity(setField),
                Name = Constants.CURSOR,
                MutatorFunc = CursorUtility.CreateCursor,
                TemplateFunc = (tableAlias) => $"CONCAT({tableAlias}.[{entity.PrimaryKeyField.DbColumnName}], '|', '{entity.DbTableName}')"
            };
        }
    }

    public enum FieldType
    {
        Scalar,
        Row,
        Set,
        Connection,
        TotalCount,
        Edge,
        Node,
        Cursor
    }
}
