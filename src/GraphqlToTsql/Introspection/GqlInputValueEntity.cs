﻿using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Introspection
{
    //type __InputValue {
    //  name: String!
    //  description: String
    //  type: __Type!
    //  defaultValue: String
    //}

    internal class GqlInputValueEntity : EntityBase
    {
        public static GqlInputValueEntity Instance = new GqlInputValueEntity();

        public override string Name => "inputValue";
        public override string DbTableName => "GqlInputValue";
        public override string EntityType => "__InputValue";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "parentTypeKey", "ParentTypeKey", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "fieldName", "FieldName", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),
                Field.Column(this, "typeKey", "TypeKey", ValueType.String, IsNullable.No, Visibility.Hidden),

                Field.Row(GqlTypeEntity.Instance, "type", new Join(
                    () => this.GetField("typeKey"),
                    () => GqlTypeEntity.Instance.GetField("key")),
                    IsNullable.No
                ),

                Field.Column(this, "defaultValue", "DefaultValue", ValueType.String, IsNullable.Yes)
            };
        }
    }
}
