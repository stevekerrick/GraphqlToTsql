using DemoEntities;
using GraphqlToTsql;
using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GraphqlToTsqlTests
{
    public abstract class IntegrationTestBase
    {
        private IServiceProvider _services;

        [OneTimeSetUp]
        public void Init()
        {
            var configuration = GetIConfigurationRoot(TestContext.CurrentContext.TestDirectory);

            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddSingleton((IConfiguration)configuration)
                .AddTransient<IGqlListener, GqlListener>()
                .AddTransient<IParser, Parser>()
                .AddTransient<IQueryTreeBuilder, QueryTreeBuilder>()
                .AddTransient<IGraphqlActions, GraphqlActions>()
                .AddTransient<IDataMutator, DataMutator>()
                .AddTransient<IJsonValueConverter, JsonValueConverter>();

            _services = serviceCollection.BuildServiceProvider();
        }

        protected T GetService<T>()
        {
            return _services.GetService<T>();
        }

        internal static ITsqlBuilder GetTsqlBuilder(List<EntityBase> allEntities)
        {
            var tsqlBuilder = new TsqlBuilder(allEntities, EmptySetBehavior.Null);
            return tsqlBuilder;
        }

        protected string GetConnectionString()
        {
            var configuration = GetService<IConfiguration>();
            return configuration["ConnectionString"];
        }

        private IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }

        // The QueryBuilder is also exercised in the Parse step, and that's really where most of the errors are being found
        protected void ParseShouldFail(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            string partialErrorMessage)
        {
            var parser = GetService<IParser>();
            var parseResult = parser.ParseGraphql(graphql, graphqlParameters, DemoEntityList.All());
            Console.WriteLine(parseResult.ParseError);

            Assert.IsNotNull(parseResult.ParseError, "Expected parse to fail, but it succeeded");
            Assert.IsTrue(parseResult.ParseError.Contains(partialErrorMessage),
                $"Unexpected error message. Expected [{partialErrorMessage}] but found [{parseResult.ParseError}]");
        }

        protected void TsqlGenerationShouldFail(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            string partialErrorMessage)
        {
            var allEntities = DemoEntityList.All();

            var parser = GetService<IParser>();
            var parseResult = parser.ParseGraphql(graphql, graphqlParameters, allEntities);
            Assert.IsNull(parseResult.ParseError, $"Parse failed: {parseResult.ParseError}");

            var tsqlBuilder = GetTsqlBuilder(allEntities);
            var tsqlResult = tsqlBuilder.Build(parseResult);
            Assert.IsNotNull(tsqlResult.Error, "Expected TSQL generation to fail, but it succeeded");
            Assert.IsTrue(tsqlResult.Error.Contains(partialErrorMessage),
                $"Unexpected error message. Expected [{partialErrorMessage}] but found [{tsqlResult.Error}]");
        }
    }
}
