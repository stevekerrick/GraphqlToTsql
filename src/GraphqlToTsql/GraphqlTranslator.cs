using GraphqlToTsql.Database;
using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphqlToTsql
{
    public interface IGraphqlTranslator
    {
        Task<TranslateResult> Translate(string graphQl, Dictionary<string, object> graphqlParameters, List<EntityBase> entityList);
    }

    public class GraphqlTranslator : IGraphqlTranslator
    {
        private readonly IParser _parser;
        private readonly ITsqlBuilder _tsqlBuilder;
        private readonly IDbAccess _dbAccess;

        public GraphqlTranslator(
            IParser parser,
            ITsqlBuilder tsqlBuilder,
            IDbAccess dbAccess)
        {
            _parser = parser;
            _tsqlBuilder = tsqlBuilder;
            _dbAccess = dbAccess;
        }

        public async Task<TranslateResult> Translate(string graphQl, Dictionary<string, object> graphqlParameters, List<EntityBase> entityList)
        {
            var parseResult = _parser.ParseGraphql(graphQl, graphqlParameters, entityList);
            if (parseResult.ParseError != null)
            {
                return new TranslateResult { ParseError = parseResult.ParseError };
            }

            var tsqlResult = _tsqlBuilder.Build(parseResult);
            if (tsqlResult.TsqlError != null)
            {
                return new TranslateResult { ParseError = tsqlResult.TsqlError };
            }

            var dbResult = await _dbAccess.QueryAsync(tsqlResult.Tsql, tsqlResult.TsqlParameters);
            return new TranslateResult
            {
                Tsql = tsqlResult.Tsql,
                TsqlParameters = tsqlResult.TsqlParameters,
                DataJson = dbResult.DataJson,
                DbError = dbResult.DbError
            };
        }
    }
}
