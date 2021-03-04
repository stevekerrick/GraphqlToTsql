using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

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

        public override string Name => "__type";
        public override string DbTableName => "GqlType";
        public override string EntityType => "__Type";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "key", "Key", ValueType.String, IsNullable.No, Visibility.Hidden),
                Field.Column(this, "kind", "Kind", ValueType.String, IsNullable.No),
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.Yes),
                Field.Column(this, "ofTypeKey", "OfTypeKey", ValueType.String, IsNullable.Yes, Visibility.Hidden),
                Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),

                Field.Set(GqlFieldDef.Instance, "fields", IsNullable.Yes, new Join(
                    () => this.GetField("key"),
                    () => GqlFieldDef.Instance.GetField("parentTypeKey")),
                    ListCanBeEmpty.No
                ),

                Field.CalculatedSet(GqlTypeDef.Instance, "interfaces", IsNullable.No,
                    tableAlias => "SELECT * FROM GqlType WHERE Kind = 'INTERFACE'",
                    ListCanBeEmpty.No),
                Field.CalculatedSet(GqlTypeDef.Instance, "possibleTypes", IsNullable.Yes,
                    tableAlias => "SELECT * FROM GqlType WHERE 1 = 0",
                    ListCanBeEmpty.No),

                Field.Set(GqlEnumValueDef.Instance, "enumValues", IsNullable.Yes, new Join(
                    () => this.GetField("key"),
                    () => GqlEnumValueDef.Instance.GetField("enumTypeKey")),
                    ListCanBeEmpty.No
                ),
                Field.CalculatedSet(GqlInputValueDef.Instance, "inputFields", IsNullable.Yes,
                    tableAlias => $"SELECT * FROM GqlInputValue iv WHERE iv.ParentTypeKey = {tableAlias}.[Key] AND iv.FieldName IS NULL",
                    ListCanBeEmpty.No
                ),
                Field.Row(GqlTypeDef.Instance, "ofType", new Join(
                    ()=>this.GetField("ofTypeKey"),
                    ()=>GqlTypeDef.Instance.GetField("key"))
                )
            };
        }

        public override string SqlDefinition => IntrospectionData.GetTypesSql();
    }
}
