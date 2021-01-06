using DemoEntities;
using GraphqlToTsql;
using GraphqlToTsql.Database;
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
                .AddTransient<IConnectionStringProvider, TestConnectionStringProvider>()
                .AddTransient<IDbAccess, DbAccess>()
                .AddTransient<IListener, Listener>()
                .AddTransient<IParser, Parser>()
                .AddTransient<IQueryTreeBuilder, QueryTreeBuilder>()
                .AddTransient<IRunner, Runner>()
                .AddTransient<ITsqlBuilder, TsqlBuilder>()
                .AddTransient<IDataMutator, DataMutator>();

            _services = serviceCollection.BuildServiceProvider();
        }

        protected TsqlResult Translate(string graphql, Dictionary<string, object> graphqlParameters)
        {
            var parser = GetService<IParser>();
            var parseResult = parser.ParseGraphql(graphql, graphqlParameters, DemoEntityList.All());
            Assert.IsNull(parseResult.ParseError, $"Parse failed: {parseResult.ParseError}");

            var tsqlBuilder = GetService<ITsqlBuilder>();
            var tsqlResult = tsqlBuilder.Build(parseResult);
            Assert.IsNull(tsqlResult.TsqlError, $"TSQL generation failed: {tsqlResult.TsqlError}");

            return tsqlResult;
        }

        protected T GetService<T>()
        {
            return _services.GetService<T>();
        }

        private IConfigurationRoot GetIConfigurationRoot(string outputPath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
    }
}
