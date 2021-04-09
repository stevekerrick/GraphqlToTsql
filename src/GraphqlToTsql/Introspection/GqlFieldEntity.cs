using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Introspection
{
    //type __Field {
    //  name: String!
    //  description: String
    //  args: [__InputValue!]!
    //  type: __Type!
    //  isDeprecated: Boolean!
    //  deprecationReason: String
    //}

    internal class GqlFieldEntity : EntityBase
    {
        public static GqlFieldEntity Instance = new GqlFieldEntity();

        public override string Name => "field";
        public override string DbTableName => "GqlField";
        public override string EntityType => "__Field";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "parentTypeKey", "ParentTypeKey", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),
                Field.Column(this, "typeKey", "TypeKey", ValueType.String, IsNullable.No, Visibility.Hidden),

                Field.CalculatedSet(GqlInputValueEntity.Instance, "args", IsNullable.No,
                    tableAlias => $"SELECT * FROM GqlInputValue iv WHERE iv.ParentTypeKey = {tableAlias}.ParentTypeKey AND iv.FieldName = {tableAlias}.Name",
                    ListCanBeEmpty.No
                ),
                Field.Row(GqlTypeEntity.Instance, "type", new Join(
                    () => this.GetField("typeKey"),
                    () => GqlTypeEntity.Instance.GetField("key")),
                    IsNullable.No
                ),

                Field.Column(this, "isDeprecated", "IsDeprecated", ValueType.Boolean, IsNullable.No),
                Field.Column(this, "deprecationReason", "DeprecationReason", ValueType.String, IsNullable.Yes)
            };
        }
    }
}
