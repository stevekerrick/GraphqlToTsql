using GraphqlToTsql;
using GraphqlToTsql.Database;
using GraphqlToTsql.Translator;
using GraphqlToTsql.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;

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
                .AddTransient<ICursorUtility, CursorUtility>()
                .AddTransient<IDbAccess, DbAccess>()
                .AddTransient<IHashUtility, HashUtility>()
                .AddTransient<IListener, Listener>()
                .AddTransient<IParser, Parser>()
                .AddTransient<IQueryTreeBuilder, QueryTreeBuilder>()
                .AddTransient<IRunner, Runner>()
                .AddTransient<ITsqlBuilder, TsqlBuilder>();

            _services = serviceCollection.BuildServiceProvider();
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
