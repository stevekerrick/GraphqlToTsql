using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System;
using System.Collections.Generic;
using ValueType = GraphqlToTsql.Translator.ValueType;

namespace GraphqlToTsql.Introspection
{
    //type __Directive {
    //  name: String!
    //  description: String
    //  locations: [__DirectiveLocation!]!
    //  args: [__InputValue!]!
    //}

    public class GqlDirectiveDef : EntityBase
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

                // TODO: locations
                //Field.CalculatedSet(???.Instance, "locations", 
                //    tableAlias => "TODO"
                //),

                //TODO: args
                Field.Set(GqlInputValueDef.Instance, "args", new Join(
                    () => this.GetField("name"),
                    () => GqlInputValueDef.Instance.GetField("directiveName"))
                )
            };
        }

        public override string SqlDefinition => IntrospectionData.GetDirectivesSql();
    }
}
