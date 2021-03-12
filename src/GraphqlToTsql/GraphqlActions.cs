using GraphqlToTsql.Database;
using GraphqlToTsql.Entities;
using GraphqlToTsql.Introspection;
using GraphqlToTsql.Translator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GraphqlToTsql
{
    public interface IGraphqlActions
    {
        /// <summary>
        /// Parse the source GraghQL and generate TSQL
        /// </summary>
        /// <param name="graphql">The GraphQL query to process</param>
        /// <param name="graphqlParameters">Variables for the GraphQL query, or null</param>
        /// <param name="settings">Settings. This is required.</param>
        /// <returns>The result of processing, which is the GraphQL converted to TSQL, or an error message.</returns>
        TsqlResult TranslateToTsql(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            GraphqlActionSettings settings);

        /// <summary>
        /// Parse the source GraghQL, generate TSQL, and run the query
        /// </summary>
        /// <param name="graphql">The GraphQL query to process</param>
        /// <param name="graphqlParameters">Variables for the GraphQL query, or null</param>
        /// <param name="settings">Settings. This is required.</param>
        /// <returns>The result, which is data returned from the database along with the TSQL command that
        /// was executed, or else an error message generated during the translation or an error message sent back
        /// by the datasbase.</returns>
        Task<QueryResult> TranslateAndRunQuery(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            GraphqlActionSettings settings);
    }

    public class GraphqlActions : IGraphqlActions
    {
        public TsqlResult TranslateToTsql(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            GraphqlActionSettings settings)
        {
            CheckSettings(settings, connectionStringNeeded: false);

            var allEntities = GetAllEntities(settings);

            // Parse the GraphQL, producing a query tree
            var parser = GetParser();
            var parseResult = parser.ParseGraphql(graphql, graphqlParameters, allEntities);
            if (parseResult.ParseError != null)
            {
                return new TsqlResult { Error = parseResult.ParseError };
            }

            // Build the data structures needed for Introspection.
            // TODO: Only do this if the query actually uses Introspection
            // TODO: Bug: using different EntityLists would cause problems
            if (settings.AllowIntrospection)
            {
                IntrospectionData.Initialize(allEntities);
            }

            // Create TSQL
            var tsqlBuilder = GetTsqlBuilder();
            var tsqlResult = tsqlBuilder.Build(parseResult);
            return tsqlResult;
        }

        public async Task<QueryResult> TranslateAndRunQuery(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            GraphqlActionSettings settings)
        {
            CheckSettings(settings, connectionStringNeeded: true);

            var sw = new Stopwatch();

            var allEntities = GetAllEntities(settings);

            // Parse the GraphQL, producing a query tree
            sw.Restart();
            var parser = GetParser();
            var parseResult = parser.ParseGraphql(graphql, graphqlParameters, allEntities);
            if (parseResult.ParseError != null)
            {
                return new QueryResult { TranslationError = parseResult.ParseError };
            }
            var parseElapsedTime = sw.ElapsedMilliseconds;

            // Build the data structures needed for Introspection.
            // TODO: Only do this if the query actually uses Introspection
            // TODO: Bug: using different EntityLists would cause problems
            if (settings.AllowIntrospection)
            {
                IntrospectionData.Initialize(allEntities);
            }

            // Create TSQL
            sw.Restart();
            var tsqlBuilder = GetTsqlBuilder();
            var tsqlResult = tsqlBuilder.Build(parseResult);
            if (tsqlResult.Error != null)
            {
                return new QueryResult { TranslationError = tsqlResult.Error };
            }
            var tsqlElapsedTime = sw.ElapsedMilliseconds;

            // Execute the TSQL
            sw.Restart();
            var dbAccess = GetDbAccess(settings.ConnectionString);
            var dbResult = await dbAccess.QueryAsync(tsqlResult.Tsql, tsqlResult.TsqlParameters);
            var dbElapsedTime = sw.ElapsedMilliseconds;

            // Perform targeted field-level mutations in the data
            var mutationsElapsedTime = (long?)null;
            if (dbResult.DbError == null)
            {
                sw.Restart();
                var dataMutator = GetDataMutator();
                dbResult.DataJson = dataMutator.Mutate(dbResult.DataJson, parseResult.RootTerm);
                mutationsElapsedTime = sw.ElapsedMilliseconds;
            }

            // Gather statistics
            var statistics = new List<Statistic>
            {
                new Statistic("1. Parse GraphQL (ms)", parseElapsedTime),
                new Statistic("2. Form TSQL (ms)", tsqlElapsedTime),
                new Statistic("3. Execute TSQL (ms)", dbElapsedTime),
                new Statistic(" . . . Actual Database Time (ms)", dbResult.DatabaseQueryTime),
                new Statistic(" . . . Query Size (chars)", tsqlResult.Tsql.Length),
                new Statistic(" . . . Result Size (chars)", dbResult.DataJson?.Length),
                new Statistic("4. Tweak Result (ms)", mutationsElapsedTime)
            };

            return new QueryResult
            {
                Tsql = tsqlResult.Tsql,
                TsqlParameters = tsqlResult.TsqlParameters,
                DataJson = dbResult.DataJson,
                DbError = dbResult.DbError,
                Statistics = statistics
            };
        }

        private static void CheckSettings(GraphqlActionSettings settings, bool connectionStringNeeded)
        {
            if (settings == null)
            {
                throw new Exception("settings can not be null");
            }

            if (settings.EntityList == null || settings.EntityList.Count == 0)
            {
                throw new Exception("settings.EntityList can not be null or empty");
            }

            if (connectionStringNeeded && string.IsNullOrEmpty(settings.ConnectionString))
            {
                throw new Exception("settings.ConnectionString can not be null or empty");
            }
        }

        private List<EntityBase> GetAllEntities(GraphqlActionSettings settings)
        {
            var allEntities = new List<EntityBase>();
            allEntities.AddRange(settings.EntityList);
            if (settings.AllowIntrospection)
            {
                allEntities.AddRange(IntrospectionEntityList.All());
            }

            return allEntities;
        }

        private static IParser GetParser()
        {
            var queryTreeBuilder = new QueryTreeBuilder();
            var listener = new Listener(queryTreeBuilder);
            var parser = new Parser(listener);
            return parser;
        }

        private static ITsqlBuilder GetTsqlBuilder()
        {
            var tsqlBuilder = new TsqlBuilder();
            return tsqlBuilder;
        }

        private static IDbAccess GetDbAccess(string connectionString)
        {
            var dbAccess = new DbAccess(connectionString);
            return dbAccess;
        }

        private static IDataMutator GetDataMutator()
        {
            var dataMutator = new DataMutator();
            return dataMutator;
        }
    }
}
