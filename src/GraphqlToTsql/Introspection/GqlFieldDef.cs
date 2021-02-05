using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Translator.ValueType;

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

    public class GqlFieldDef : EntityBase
    {
        public static GqlFieldDef Instance = new GqlFieldDef();

        public override string Name => "field";
        public override string DbTableName => "GqlField";
        public override string EntityType => "__Field";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),
                Field.Column(this, "typeName", "TypeName", ValueType.String, IsNullable.No),

                Field.Set(GqlInputValueDef.Instance, "args", new Join(
                    () => this.GetField("name"),
                    () => GqlInputValueDef.Instance.GetField("fieldName"))
                ),
                Field.Row(GqlTypeDef.Instance, "type", new Join(
                    () => this.GetField("typeName"),
                    () => GqlTypeDef.Instance.GetField("name"))
                ),

                Field.Column(this, "isDeprecated", "IsDeprecated", ValueType.Boolean, IsNullable.No),
                Field.Column(this, "deprecationReason", "DeprecationReason", ValueType.String, IsNullable.Yes)
            };
        }
    }
}
