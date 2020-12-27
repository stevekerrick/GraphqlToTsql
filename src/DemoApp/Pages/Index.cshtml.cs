using DemoEntities;
using GraphqlToTsql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly IGraphqlTranslator _graphqlTranslator;

        public IndexModel(
            ILogger<IndexModel> logger,
            IConfiguration configuration,
            IGraphqlTranslator graphqlTranslator)
        {
            _logger = logger;
            _configuration = configuration;
            _graphqlTranslator = graphqlTranslator;
        }

        public void OnGet()
        {
        }

        public async Task<JsonResult> OnPostRunQuery([FromBody] QueryModel query)
        {
            var result = await RunQuery(query);
            return new JsonResult(result);
        }

        private async Task<QueryResult> RunQuery(QueryModel query)
        {
            var graphql = query.Graphql;
            var graphqlParameters = string.IsNullOrEmpty(query.GraphqlParametersJson)
                ? null
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(query.GraphqlParametersJson);

            var translateResult = await _graphqlTranslator.Translate(graphql, graphqlParameters, DemoEntityList.All());

            return new QueryResult
            {
                Tsql = translateResult.Tsql,
                TsqlParametersJson = ToFormattedJson(translateResult.TsqlParameters),
                DataJson = ToFormattedJson(Deserialize(translateResult.DataJson)),
                Error = translateResult.ParseError ?? translateResult.DbError
            };
        }

        private static object Deserialize(string json)
        {
            if (json == null) return null;
            return JsonConvert.DeserializeObject(json);
        }

        private static string ToFormattedJson(object obj)
        {
            if (obj == null) return null;
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        private class QueryResult
        {
            public string Tsql { get; set; }
            public string TsqlParametersJson { get; set; }
            public string DataJson { get; set; }
            public string Error { get; set; }
            public bool IsSuccess => Error == null;
        }
    }

    public class QueryModel
    {
        public string Graphql { get; set; }
        public string GraphqlParametersJson { get; set; }
    }
}
