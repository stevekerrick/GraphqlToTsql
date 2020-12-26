using DemoEntities;
using GraphqlToTsql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

            GraphqlToTsql.Translator.TranslateResult translateResult;
            var result = new QueryResult();

            // Build SQL command
            try
            {
                var translator = new GraphqlTranslator();
                translateResult = translator.Translate(graphql, graphqlParameters, DemoEntityList.All());
                if (!translateResult.IsSuccessful)
                {
                    result.Error = translateResult.ParseError;
                    return result;
                }
                result.Tsql = translateResult.Tsql;
                result.TsqlParametersJson = ToFormattedJson(translateResult.TsqlParameters);
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
                var json = await DbAccess.QueryAsync(connectionString, translateResult.Tsql, translateResult.TsqlParameters);
                result.DataJson = ToFormattedJson(Deserialize(json));
            }
            catch (Exception e)
            {
                result.Error = $"Database error: {e.Message}";
                return result;
            }

            return result;
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
