using GraphqlToTsql.Database;
using GraphqlToTsql.Entities;
using GraphqlToTsql.Introspection;
using GraphqlToTsql.Translator;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GraphqlToTsql
{
    /// <summary>
    /// Parse the source GraghQL, generate TSQL, and run the query
    /// </summary>
    public interface IRunner
    {
        Task<RunnerResult> TranslateAndRun(string graphql, Dictionary<string, object> graphqlParameters, List<EntityBase> entityList);
    }

    public class Runner : IRunner
    {
        private readonly IParser _parser;
        private readonly ITsqlBuilder _tsqlBuilder;
        private readonly IDbAccess _dbAccess;
        private readonly IDataMutator _dataMutator;

        public Runner(
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

        public async Task<RunnerResult> TranslateAndRun(
            string graphql,
            Dictionary<string, object> graphqlParameters,
            List<EntityBase> entityList)
        {
            const bool enableIntrospection = true;

            var sw = new Stopwatch();

            var allEntities = new List<EntityBase>();
            allEntities.AddRange(entityList);

            // Set up Introspection
            if (enableIntrospection)
            {
                allEntities.AddRange(IntrospectionEntityList.All());
                IntrospectionData.Initialize(allEntities);
            }

            // Parse the GraphQL, producing a query tree
            sw.Restart();
            var parseResult = _parser.ParseGraphql(graphql, graphqlParameters, allEntities);
            if (parseResult.ParseError != null)
            {
                return new RunnerResult { ParseError = parseResult.ParseError };
            }
            var parseElapsedTime = sw.ElapsedMilliseconds;

            // Create TSQL
            sw.Restart();
            var tsqlResult = _tsqlBuilder.Build(parseResult);
            if (tsqlResult.TsqlError != null)
            {
                return new RunnerResult { ParseError = tsqlResult.TsqlError };
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

            return new RunnerResult
            {
                Tsql = tsqlResult.Tsql,
                TsqlParameters = tsqlResult.TsqlParameters,
                DataJson = dbResult.DataJson,
                DbError = dbResult.DbError,
                Statistics = statistics
            };
        }
    }
}
