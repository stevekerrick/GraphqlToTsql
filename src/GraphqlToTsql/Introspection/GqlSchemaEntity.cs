using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
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

    internal class GqlSchemaEntity : EntityBase
    {
        public static GqlSchemaEntity Instance = new GqlSchemaEntity();

        public override string Name => "__schema";
        public override string DbTableName => "GqlSchema";
        public override string EntityType => "__Schema";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Schema inspection shouldn't need PKs");

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.CalculatedSet(GqlTypeEntity.Instance, "types", IsNullable.No,
                    tableAlias => "SELECT * FROM GqlType WHERE name IS NOT NULL",
                    ListCanBeEmpty.No),

                Field.CalculatedRow(GqlTypeEntity.Instance, "queryType",
                    tableAlias => "SELECT * FROM GqlType WHERE name = 'Query'",
                    IsNullable.No),
                Field.CalculatedRow(GqlTypeEntity.Instance, "mutationType",
                    tableAlias => "SELECT * FROM GqlType WHERE name = 'Mutation'"),
                Field.CalculatedRow(GqlTypeEntity.Instance, "subscriptionType",
                    tableAlias => "SELECT * FROM GqlType WHERE name = 'Subscription'"),

                Field.CalculatedSet(GqlDirectiveEntity.Instance, "directives", IsNullable.No,
                    tableAlias => "SELECT * FROM GqlDirective",
                    ListCanBeEmpty.No)
            };
        }

        public override string SqlDefinition => "SELECT 'hello' AS UnusedColumn";
    }
}
