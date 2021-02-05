using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Translator.ValueType;

namespace GraphqlToTsql.Introspection
{
    //type __Type {
    //  kind: __TypeKind!
    //  name: String
    //  description: String

    //  # OBJECT and INTERFACE only
    //  fields(includeDeprecated: Boolean = false): [__Field!]

    //  # OBJECT only
    //  interfaces: [__Type!]

    //  # INTERFACE and UNION only
    //  possibleTypes: [__Type!]

    //  # ENUM only
    //  enumValues(includeDeprecated: Boolean = false): [__EnumValue!]

    //  # INPUT_OBJECT only
    //  inputFields: [__InputValue!]

    //  # NON_NULL and LIST only
    //  ofType: __Type
    //}

    public class GqlTypeDef : EntityBase
    {
        public static GqlTypeDef Instance = new GqlTypeDef();

        public override string Name => "type";
        public override string DbTableName => "GqlType";
        public override string EntityType => "__Type";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "kind", "Kind", ValueType.String, IsNullable.No),
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.Yes),
                Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),

                Field.Set(GqlFieldDef.Instance, "fields", new Join(
                    () => this.GetField("name"),
                    () => GqlFieldDef.Instance.GetField("typeName"))
                ),

                Field.CalculatedSet(GqlTypeDef.Instance, "interfaces",
                    tableAlias => "SELECT * FROM GqlType WHERE 1 = 0"),
                Field.CalculatedSet(GqlTypeDef.Instance, "possibleTypes",
                    tableAlias => "SELECT * FROM GqlType WHERE 1 = 0"),

                Field.Set(GqlEnumValueDef.Instance, "enumFields", new Join(
                    () => this.GetField("name"),
                    () => GqlEnumValueDef.Instance.GetField("enumName"))
                ),
                Field.Set(GqlInputValueDef.Instance, "inputFields", new Join(
                    () => this.GetField("name"),
                    () => GqlInputValueDef.Instance.GetField("inputObjectName"))
                ),
                Field.CalculatedRow(GqlTypeDef.Instance, "ofType",
                    tableAlias => "TODO")
            };
        }
    }
}
