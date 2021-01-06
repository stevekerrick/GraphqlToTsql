using DemoEntities;
using GraphqlToTsql;
using GraphqlToTsql.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IRunner _runner;

        public IndexModel(
            ILogger<IndexModel> logger,
            IRunner runner)
        {
            _logger = logger;
            _runner = runner;
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

            var runnerResult = await _runner.TranslateAndRun(graphql, graphqlParameters, DemoEntityList.All());

            return new QueryResult
            {
                Tsql = runnerResult.Tsql,
                TsqlParametersJson = ToFormattedJson(runnerResult.TsqlParameters),
                DataJson = ToFormattedJson(Deserialize(runnerResult.DataJson)),
                Error = runnerResult.ParseError ?? runnerResult.DbError,
                Statistics = runnerResult.Statistics
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
            public List<Statistic> Statistics { get; set; }
        }
    }

    public class QueryModel
    {
        public string Graphql { get; set; }
        public string GraphqlParametersJson { get; set; }
    }
}
