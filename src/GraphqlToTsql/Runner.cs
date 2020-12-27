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

        public Runner(
            IParser parser,
            ITsqlBuilder tsqlBuilder,
            IDbAccess dbAccess)
        {
            _parser = parser;
            _tsqlBuilder = tsqlBuilder;
            _dbAccess = dbAccess;
        }

        public async Task<RunnerResult> TranslateAndRun(string graphQl, Dictionary<string, object> graphqlParameters, List<EntityBase> entityList)
        {
            var parseResult = _parser.ParseGraphql(graphQl, graphqlParameters, entityList);
            if (parseResult.ParseError != null)
            {
                return new RunnerResult { ParseError = parseResult.ParseError };
            }

            var tsqlResult = _tsqlBuilder.Build(parseResult);
            if (tsqlResult.TsqlError != null)
            {
                return new RunnerResult { ParseError = tsqlResult.TsqlError };
            }

            var dbResult = await _dbAccess.QueryAsync(tsqlResult.Tsql, tsqlResult.TsqlParameters);
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
