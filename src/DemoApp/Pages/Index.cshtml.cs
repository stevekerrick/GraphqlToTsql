using Dapper;
using DemoEntities;
using GraphqlToTsql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DemoApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
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

            var result = new QueryResult();

            // Build SQL command
            try
            {
                var entityList = new DemoEntityList();
                var translator = new GraphqlTranslator(entityList);
                var translateResult = translator.Translate(graphql, graphqlParameters);
                if (!translateResult.IsSuccessful)
                {
                    result.Error = translateResult.ParseError;
                    return result;
                }
                result.Tsql = translateResult.Tsql;
                result.TsqlParameters = translateResult.TsqlParameters;
            }
            catch (Exception e)
            {
                result.Error = $"Error during parse: {e.Message}";
                return result;
            }

            // Execute the SQL
            try
            {
                var connectionString = _configuration["ConnectionString"];
                var json = await DbAccess.QueryAsync(connectionString, result.Tsql, result.TsqlParameters);
                result.Data = JsonConvert.DeserializeObject(json);
            }
            catch (Exception e)
            {
                result.Error = $"Database error: {e.Message}";
                return result;
            }

            return result;
        }

        private class QueryResult
        {
            public string Tsql { get; set; }
            public Dictionary<string, object> TsqlParameters { get; set; }
            public object Data { get; set; }
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
