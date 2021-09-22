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
                .AddTransient<IListener, Listener>()
                .AddTransient<IParser, Parser>()
                .AddTransient<IQueryTreeBuilder, QueryTreeBuilder>()
                .AddTransient<IGraphqlActions, GraphqlActions>()
                .AddTransient<IDataMutator, DataMutator>();

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
    }
}
