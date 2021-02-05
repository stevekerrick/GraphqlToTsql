using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Translator.ValueType;

namespace GraphqlToTsql.Introspection
{
    //type __InputValue {
    //  name: String!
    //  description: String
    //  type: __Type!
    //  defaultValue: String
    //}

    public class GqlInputValueDef : EntityBase
    {
        public static GqlInputValueDef Instance = new GqlInputValueDef();

        public override string Name => "inputValue";
        public override string DbTableName => "GqlInputValue";
        public override string EntityType => "__InputValue";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),
                Field.Column(this, "typeName", "TypeName", ValueType.String, IsNullable.No),

                Field.Row(GqlTypeDef.Instance, "type", new Join(
                    () => this.GetField("typeName"),
                    () => GqlTypeDef.Instance.GetField("name"))
                ),

                Field.Column(this, "defaultValue", "DefaultValue", ValueType.String, IsNullable.Yes)
            };
        }
    }
}
