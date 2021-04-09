using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Introspection
{
    //type __EnumValue {
    //  name: String!
    //  description: String
    //  isDeprecated: Boolean!
    //  deprecationReason: String
    //}

    internal class GqlEnumValueEntity : EntityBase
    {
        public static GqlEnumValueEntity Instance = new GqlEnumValueEntity();

        public override string Name => "enumValue";
        public override string DbTableName => "GqlEnumValue";
        public override string EntityType => "__EnumValue";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "enumTypeKey", "EnumTypeKey", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),
                Field.Column(this, "isDeprecated", "IsDeprecated", ValueType.Boolean, IsNullable.No),
                Field.Column(this, "deprecationReason", "DeprecationReason", ValueType.String, IsNullable.Yes)
            };
        }
    }
}
