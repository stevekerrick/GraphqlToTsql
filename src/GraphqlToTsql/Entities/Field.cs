﻿using GraphqlToTsql.Util;
using System;
using System.Diagnostics;

namespace GraphqlToTsql.Entities
{
    /// <summary>
    /// Each of the fields you want to expose in GraphQL will need a Field instance.
    /// You'll instantiate your Fields in the BuildFieldList method of your entities,
    /// by using one of these factory methods: Column, CalculatedField, Row, Set,
    /// CalculatedRow, and CalculatedSet.
    /// </summary>
    [DebuggerDisplay("{Name,nq}")]
    public class Field
    {
        internal EntityBase Entity { get; private set; }
        internal string Name { get; private set; }
        internal FieldType FieldType { get; private set; }
        internal string DbColumnName { get; private set; }
        internal ValueType ValueType { get; private set; }
        internal IsNullable? IsNullable { get; private set; }
        internal ListCanBeEmpty? IsNonEmptyList { get; private set; }
        internal Join Join { get; private set; }
        internal Func<string, string> TemplateFunc { get; private set; }
        internal Func<string, string> MutatorFunc { get; private set; }

        private Field() { }

        /// <summary>
        /// Builds a field that maps to a database column. This is the factory method you'll use most often.
        /// </summary>
        /// <param name="entity">The entity this field belongs to</param>
        /// <param name="name">The name of the field in the GraphQL</param>
        /// <param name="dbColumnName">The column name in the database</param>
        /// <param name="valueType">Data type of the column. One of: String, Int, Float, Boolean.</param>
        /// <param name="isNullable">Is the database column nullable?</param>
        public static Field Column(EntityBase entity, string name, string dbColumnName, ValueType valueType, IsNullable isNullable) => new Field
        {
            FieldType = FieldType.Column,
            Entity = entity,
            Name = name,
            DbColumnName = dbColumnName,
            ValueType = valueType,
            IsNullable = isNullable
        };

        /// <summary>
        /// Builds a field that uses custom SQL. Use this to create a GraphQL field that doesn't map
        /// to any database column.
        /// </summary>
        /// <param name="entity">The entity this field belongs to</param>
        /// <param name="name">The name of the field in the GraphQL</param>
        /// <param name="valueType">Data type of the column. One of: String, Int, Float, Boolean.</param>
        /// <param name="isNullable">Can the custom SQL result in a null value?</param>
        /// <param name="templateFunc">Function that takes the table alias, and returns a SQL SELECT statement.
        /// <example>For example:
        /// <code>(tableAlias) => $"SELECT SUM(od.Quantity) FROM OrderDetail od WHERE {tableAlias}.[Name] = od.ProductName"</code>
        /// </example>
        /// </param>
        public static Field CalculatedField(EntityBase entity, string name, ValueType valueType, IsNullable isNullable,
            Func<string, string> templateFunc) => new Field
            {
                FieldType = FieldType.Column,
                Entity = entity,
                Name = name,
                ValueType = valueType,
                IsNullable = isNullable,
                TemplateFunc = templateFunc
            };

        /// <summary>
        /// Builds a field for a child entity.
        /// </summary>
        /// <param name="entity">Tne entity of the child</param>
        /// <param name="name">The name of the field in the GraphQL</param>
        /// <param name="join">Join criteria between the parent and child entities</param>
        /// <param name="isNullable">Can the field be null? This setting is usually only used on the system types used for introspection.</param>
        public static Field Row(EntityBase entity, string name, Join join, IsNullable isNullable = Entities.IsNullable.Yes) => new Field
        {
            FieldType = FieldType.Row,
            Entity = entity,
            Name = name,
            Join = join,
            IsNullable = isNullable
        };

        /// <summary>
        /// Builds a field for one-to-many set.
        /// </summary>
        /// <param name="entity">Tne entity of the children</param>
        /// <param name="name">The name of the field in the GraphQL</param>
        /// <param name="join">Join criteria between the parent and child entities</param>
        /// <param name="isNonEmptyList">Can the list be empty?  This setting is usually only used on the system types used for introspection.</param>
        public static Field Set(EntityBase entity, string name, IsNullable isNullable, Join join, ListCanBeEmpty? isNonEmptyList = null) => new Field
        {
            FieldType = FieldType.Set,
            Entity = entity,
            Name = name,
            IsNullable = isNullable,
            IsNonEmptyList = isNonEmptyList,
            Join = join
        };

        /// <summary>
        /// Builds a field for a child entity, where custom SQL is used to retrieve the child row.
        /// </summary>
        /// <param name="entity">Tne entity of the child</param>
        /// <param name="name">The name of the field in the GraphQL</param>
        /// <param name="templateFunc">Function that takes the parent table alias, and returns a SQL SELECT statement to retrieve the child row</param>
        /// <param name="isNullable">Can the field be null? This setting is usually only used on the system types used for introspection.</param>
        public static Field CalculatedRow(EntityBase entity, string name,
            Func<string, string> templateFunc, IsNullable isNullable = Entities.IsNullable.Yes) => new Field
            {
                FieldType = FieldType.Row,
                Entity = entity,
                Name = name,
                TemplateFunc = templateFunc,
                IsNullable = isNullable
            };

        /// <summary>
        /// Builds a field for a one-to-many set, where custom SQL is used to retrieve the child set.
        /// </summary>
        /// <param name="entity">Tne entity of the child</param>
        /// <param name="name">The name of the field in the GraphQL</param>
        /// <param name="templateFunc">Function that takes the parent table alias, and returns a SQL SELECT statement to retrieve the child set</param>
        /// <param name="isNonEmptyList">Can the list be empty?  This setting is usually only used on the system types used for introspection.</param>
        public static Field CalculatedSet(EntityBase entity, string name, IsNullable isNullable,
            Func<string, string> templateFunc, ListCanBeEmpty? isNonEmptyList = null) => new Field
            {
                FieldType = FieldType.Set,
                Entity = entity,
                Name = name,
                IsNullable = isNullable,
                IsNonEmptyList = isNonEmptyList,
                TemplateFunc = templateFunc
            };

        internal static Field Connection(Field setField) => new Field
        {
            FieldType = FieldType.Connection,
            Entity = new ConnectionEntity(setField),
            Name = $"{setField.Name}{Constants.CONNECTION}",
            Join = setField.Join
        };

        internal static Field TotalCount(Field setField) => new Field
        {
            FieldType = FieldType.TotalCount,
            Entity = setField.Entity,
            Name = Constants.TOTAL_COUNT,
            ValueType = ValueType.Int,
            IsNullable = Entities.IsNullable.No,
            Join = setField.Join
        };

        internal static Field Edges(Field setField) => new Field
        {
            FieldType = FieldType.Edge,
            Entity = new EdgeEntity(setField),
            Name = Constants.EDGES,
            Join = setField.Join
        };

        internal static Field Node(Field setField) => new Field
        {
            FieldType = FieldType.Node,
            Entity = new NodeEntity(setField),
            Name = Constants.NODE
        };

        internal static Field Cursor(Field setField)
        {
            var entity = setField.Entity;
            var pk = entity.SinglePrimaryKeyFieldForPaging;

            return new Field
            {
                FieldType = FieldType.Cursor,
                Entity = new NodeEntity(setField),
                Name = Constants.CURSOR,
                ValueType = ValueType.String,
                IsNullable = Entities.IsNullable.No,
                MutatorFunc = CursorUtility.CreateCursor,
                TemplateFunc = (tableAlias) => CursorUtility.TsqlCursorDataFunc(pk.ValueType, tableAlias, entity.DbTableName, pk.DbColumnName)
            };
        }

        internal static Field Directive(string name) => new Field
        {
            FieldType = FieldType.Directive,
            Entity = new DirectiveEntity(name),
            Name = name
        };
    }

    internal enum FieldType
    {
        Column,
        Row,
        Set,
        Connection,
        TotalCount,
        Edge,
        Node,
        Cursor,
        Directive
    }
}
