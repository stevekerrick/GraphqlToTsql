using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;

namespace GraphqlToTsql.Introspection
{
    //type __Schema {
    //  types: [__Type!]!
    //  queryType: __Type!
    //  mutationType: __Type
    //  subscriptionType: __Type
    //  directives: [__Directive!]!
    //}

    public class GqlSchemaDef : EntityBase
    {
        public static GqlSchemaDef Instance = new GqlSchemaDef();

        public override string Name => "__schema";
        public override string DbTableName => "GqlSchema";
        public override string EntityType => "__Schema";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.CalculatedSet(GqlTypeDef.Instance, "types",
                    tableAlias => "SELECT * FROM GqlType"),

                Field.CalculatedRow(GqlTypeDef.Instance, "queryType",
                    tableAlias => "SELECT * FROM GqlType WHERE name = 'Query'"),
                Field.CalculatedRow(GqlTypeDef.Instance, "mutationType",
                    tableAlias => "SELECT * FROM GqlType WHERE name = 'Mutation'"),
                Field.CalculatedRow(GqlTypeDef.Instance, "subscriptionType",
                    tableAlias => "SELECT * FROM GqlType WHERE name = 'Subscription'"),

                Field.CalculatedSet(GqlDirectiveDef.Instance, "directives",
                    tableAlias => "SELECT * FROM GqlDirective")
            };
        }
    }
}
