using GraphqlToTsql.Database;
using GraphqlToTsql.Entities;
using GraphqlToTsql.Introspection;
using GraphqlToTsql.Translator;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GraphqlToTsql
{
    public interface IGraphqlActions
    {
        TsqlResult TranslateToTsql(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            List<EntityBase> entityList);

        /// <summary>
        /// Parse the source GraghQL, generate TSQL, and run the query
        /// </summary>
        Task<QueryResult> TranslateAndRunQuery(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            List<EntityBase> entityList);
    }

    public class GraphqlActions : IGraphqlActions
    {
        private readonly IParser _parser;
        private readonly ITsqlBuilder _tsqlBuilder;
        private readonly IDbAccess _dbAccess;
        private readonly IDataMutator _dataMutator;

        public GraphqlActions(
            IParser parser,
            ITsqlBuilder tsqlBuilder,
            IDbAccess dbAccess,
            IDataMutator dataMutator)
        {
            _parser = parser;
            _tsqlBuilder = tsqlBuilder;
            _dbAccess = dbAccess;
            _dataMutator = dataMutator;
        }

        public TsqlResult TranslateToTsql(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            List<EntityBase> entityList)
        {
            const bool enableIntrospection = true;

            var allEntities = enableIntrospection
                ? ExpandEntityList(entityList)
                : entityList;

            // Parse the GraphQL, producing a query tree
            var parseResult = _parser.ParseGraphql(graphql, graphqlParameters, allEntities);
            if (parseResult.ParseError != null)
            {
                return new TsqlResult { Error = parseResult.ParseError };
            }

            // Build the data structures needed for Introspection.
            // TODO: Only do this if the query actually uses Introspection
            // TODO: Bug: using different EntityLists would cause problems
            // TODO: use a setting instead of hardcoded true
            if (true)
            {
                IntrospectionData.Initialize(allEntities);
            }

            // Create TSQL
            var tsqlResult = _tsqlBuilder.Build(parseResult);
            return tsqlResult;
        }

        public async Task<QueryResult> TranslateAndRunQuery(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            List<EntityBase> entityList)
        {
            const bool enableIntrospection = true;

            var sw = new Stopwatch();

            var allEntities = enableIntrospection
                ? ExpandEntityList(entityList)
                : entityList;

            // Parse the GraphQL, producing a query tree
            sw.Restart();
            var parseResult = _parser.ParseGraphql(graphql, graphqlParameters, allEntities);
            if (parseResult.ParseError != null)
            {
                return new QueryResult { TranslationError = parseResult.ParseError };
            }
            var parseElapsedTime = sw.ElapsedMilliseconds;

            // Build the data structures needed for Introspection.
            // TODO: Only do this if the query actually uses Introspection
            // TODO: Bug: using different EntityLists would cause problems
            // TODO: use a setting instead of hardcoded true
            if (true)
            {
                IntrospectionData.Initialize(allEntities);
            }

            // Create TSQL
            sw.Restart();
            var tsqlResult = _tsqlBuilder.Build(parseResult);
            if (tsqlResult.Error != null)
            {
                return new QueryResult { TranslationError = tsqlResult.Error };
            }
            var tsqlElapsedTime = sw.ElapsedMilliseconds;

            // Execute the TSQL
            sw.Restart();
            var dbResult = await _dbAccess.QueryAsync(tsqlResult.Tsql, tsqlResult.TsqlParameters);
            var dbElapsedTime = sw.ElapsedMilliseconds;

            // Perform targeted field-level mutations in the data
            var mutationsElapsedTime = (long?)null;
            if (dbResult.DbError == null)
            {
                sw.Restart();
                dbResult.DataJson = _dataMutator.Mutate(dbResult.DataJson, parseResult.RootTerm);
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

        private List<EntityBase> ExpandEntityList(List<EntityBase> entityList)
        {
            var allEntities = new List<EntityBase>();
            allEntities.AddRange(entityList);
            allEntities.AddRange(IntrospectionEntityList.All());

            return allEntities;
        }

    }
}
