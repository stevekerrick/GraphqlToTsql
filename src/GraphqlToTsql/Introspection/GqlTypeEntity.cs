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

    internal class GqlTypeEntity : EntityBase
    {
        public static GqlTypeEntity Instance = new GqlTypeEntity();

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

                Field.Set(GqlFieldEntity.Instance, "fields", IsNullable.Yes, new Join(
                    () => this.GetField("key"),
                    () => GqlFieldEntity.Instance.GetField("parentTypeKey")),
                    ListCanBeEmpty.No
                ),

                Field.CalculatedSet(GqlTypeEntity.Instance, "interfaces", IsNullable.No,
                    tableAlias => "SELECT * FROM GqlType WHERE Kind = 'INTERFACE'",
                    ListCanBeEmpty.No),
                Field.CalculatedSet(GqlTypeEntity.Instance, "possibleTypes", IsNullable.Yes,
                    tableAlias => "SELECT * FROM GqlType WHERE 1 = 0",
                    ListCanBeEmpty.No),

                Field.Set(GqlEnumValueEntity.Instance, "enumValues", IsNullable.Yes, new Join(
                    () => this.GetField("key"),
                    () => GqlEnumValueEntity.Instance.GetField("enumTypeKey")),
                    ListCanBeEmpty.No
                ),
                Field.CalculatedSet(GqlInputValueEntity.Instance, "inputFields", IsNullable.Yes,
                    tableAlias => $"SELECT * FROM GqlInputValue iv WHERE iv.ParentTypeKey = {tableAlias}.[Key] AND iv.FieldName IS NULL",
                    ListCanBeEmpty.No
                ),
                Field.Row(GqlTypeEntity.Instance, "ofType", new Join(
                    ()=>this.GetField("ofTypeKey"),
                    ()=>GqlTypeEntity.Instance.GetField("key"))
                )
            };
        }
    }
}
