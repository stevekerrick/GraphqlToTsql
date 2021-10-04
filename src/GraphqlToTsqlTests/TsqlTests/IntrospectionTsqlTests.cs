using NUnit.Framework;

namespace GraphqlToTsqlTests.TsqlTests
{
    public class IntrospectionTsqlTests : TsqlTestBase
    {
        [Test]
        public void IntrospectionGqlSchemaTableTest()
        {
            const string graphql = "{ __schema { types { name } } }";

            var result = Translate(graphql, null);

            Assert.IsTrue(result.Tsql.Contains("[GqlSchema] AS ("));
        }

        [Test]
        public void IntrospectionGqlTypeTableTest()
        {
            const string graphql = "{ __schema { types { name } } }";

            var result = Translate(graphql, null);

            Assert.IsTrue(result.Tsql.Contains("[GqlType] AS ("));
        }

        [Test]
        public void IntrospectionGqlFieldTableTest()
        {
            const string graphql = "{ __type (name: \"\") { fields { name } } }";

            var result = Translate(graphql, null);

            Assert.IsTrue(result.Tsql.Contains("[GqlField] AS ("));
        }

        [Test]
        public void IntrospectionGqlEnumValueTableTest()
        {
            const string graphql = "{ __type (name: \"\") { enumValues { name } } }";

            var result = Translate(graphql, null);

            Assert.IsTrue(result.Tsql.Contains("[GqlEnumValue] AS ("));
        }

        [Test]
        public void IntrospectionGqlDirectiveTableTest()
        {
            var graphql = @"
{
  __schema {
    directives { name }
  }
}".Trim();

            var result = Translate(graphql, null);

            Assert.IsTrue(result.Tsql.Contains("[GqlDirective] AS ("));
        }

        [Test]
        public void IntrospectionGqlInputValueTableTest()
        {
            var graphql = @"
{
  __type (name: ""SellerBadge"") {
    fields (name: ""badge"") {
      args { name }
    }
  }
}".Trim();

            var result = Translate(graphql, null);

            Assert.IsTrue(result.Tsql.Contains("[GqlInputValue] AS ("));
        }
    }
}
