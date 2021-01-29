using DemoEntities;
using GraphqlToTsql;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphqlController : ControllerBase
    {
        private readonly IRunner _runner;

        public GraphqlController(
            IRunner runner)
        {
            _runner = runner;
        }

        [HttpPost]
        public async Task<JsonResult> Post([FromBody] QueryRequest query, bool? showStatistics)
        {
            var result = await RunQuery(query);

            object response;
            if (showStatistics.GetValueOrDefault())
            {
                response = result;
            }
            else if (result.Errors != null)
            {
                response = new { result.Errors };
            }
            else
            {
                response = new { result.Data };
            }

            return new JsonResult(response);
        }

        private async Task<QueryResponse> RunQuery(QueryRequest query)
        {
            var graphql = query.Query;
            var graphqlParameters = string.IsNullOrEmpty(query.Variables)
                ? null
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(query.Variables);

            var runnerResult = await _runner.TranslateAndRun(graphql, graphqlParameters, DemoEntityList.All());

            var errors =
                runnerResult.ParseError != null ? new[] { runnerResult.ParseError }
                : runnerResult.DbError != null ? new[] { runnerResult.DbError }
                : null;

            return new QueryResponse
            {
                Data = Deserialize(runnerResult.DataJson),
                Errors = errors,
                Tsql = runnerResult.Tsql,
                TsqlParameters = runnerResult.TsqlParameters,
                Statistics = runnerResult.Statistics
            };
        }

        private static object Deserialize(string json)
        {
            if (json == null) return null;
            return JsonConvert.DeserializeObject(json);
        }
    }

    // The shape of the QueryRequest is dictated by the GraphQL standard
    public class QueryRequest
    {
        // The GraphQL query
        public string Query { get; set; }

        // The GraphQL variable values, in JSON format
        public string Variables { get; set; }
    }

    public class QueryResponse
    {
        // These parts are dictated by the GraphQL standard
        public object Data { get; set; }
        public string[] Errors { get; set; }

        // These parts are extra
        public string Tsql { get; set; }
        public Dictionary<string, object> TsqlParameters { get; set; }
        public List<Statistic> Statistics { get; set; }
    }
}
