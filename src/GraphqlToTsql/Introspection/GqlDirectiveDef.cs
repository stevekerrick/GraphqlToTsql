using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Introspection
{
    //type __Directive {
    //  name: String!
    //  description: String
    //  locations: [__DirectiveLocation!]!
    //  args: [__InputValue!]!
    //}

    internal class GqlDirectiveDef : EntityBase
    {
        public static GqlDirectiveDef Instance = new GqlDirectiveDef();

        public override string Name => "directive";
        public override string DbTableName => "GqlDirective";
        public override string EntityType => "__Directive";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Column(this, "name", "Name", ValueType.String, IsNullable.No),
                Field.Column(this, "description", "Description", ValueType.String, IsNullable.Yes),
                Field.Column(this, "locationsJson", "LocationsJson", ValueType.String, IsNullable.No, Visibility.Hidden),

                Field.CalculatedField(this, "locations", ValueType.String, IsNullable.No,
                    tableAlias => $"JSON_QUERY({tableAlias}.LocationsJson)"
                ),

                Field.CalculatedSet(GqlInputValueDef.Instance, "args", IsNullable.No,
                    tableAlias => $"SELECT * FROM GqlInputValue WHERE ParentTypeKey = '{Constants.DIRECTIVE_TYPE_KEY}' AND FieldName = {tableAlias}.Name",
                    ListCanBeEmpty.No
                )
            };
        }
    }
}
