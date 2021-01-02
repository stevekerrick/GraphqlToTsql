using GraphqlToTsql.Database;
using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphqlToTsql
{
    public interface IRunner
    {
        Task<RunnerResult> TranslateAndRun(string graphQl, Dictionary<string, object> graphqlParameters, List<EntityBase> entityList);
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

        public async Task<RunnerResult> TranslateAndRun(string graphQl, Dictionary<string, object> graphqlParameters, List<EntityBase> entityList)
        {
            // Parse the GraphQL, producing a query tree
            var parseResult = _parser.ParseGraphql(graphQl, graphqlParameters, entityList);
            if (parseResult.ParseError != null)
            {
                return new RunnerResult { ParseError = parseResult.ParseError };
            }

            // Create TSQL
            var tsqlResult = _tsqlBuilder.Build(parseResult);
            if (tsqlResult.TsqlError != null)
            {
                return new RunnerResult { ParseError = tsqlResult.TsqlError };
            }

            // Execute the TSQL
            var dbResult = await _dbAccess.QueryAsync(tsqlResult.Tsql, tsqlResult.TsqlParameters);

            // Perform targeted field-level mutations in the data
            if (dbResult.DbError == null)
            {
                dbResult.DataJson = _dataMutator.Mutate(dbResult.DataJson, parseResult.TopTerm);
            }

            return new RunnerResult
            {
                Tsql = tsqlResult.Tsql,
                TsqlParameters = tsqlResult.TsqlParameters,
                DataJson = dbResult.DataJson,
                DbError = dbResult.DbError
            };
        }
    }
}
