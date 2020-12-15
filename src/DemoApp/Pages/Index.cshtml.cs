using Dapper;
using GraphqlToTsql.Translator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
//using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
            var queryParams = string.IsNullOrEmpty(query.ParamsJson)
                ? null
                : JsonConvert.DeserializeObject<Dictionary<string, object>>(query.ParamsJson);

            var result = new QueryResult();

            // Build SQL command
            try
            {
                var translator = new GraphqlTranslator();
                var translateResult = translator.Translate(query.Query, queryParams);
                if (!translateResult.IsSuccessful)
                {
                    result.Error = translateResult.ParseError;
                    return result;
                }
                result.Sql = translateResult.Tsql;
            }
            catch (Exception e)
            {
                result.Error = $"Error during parse: {e.Message}";
                return result;
            }

            // Execute the SQL
            try
            {
                var conn = _configuration["ConnectionString"];
                using (var connection = new SqlConnection(conn))
                {
                    var json = await connection.QuerySingleOrDefaultAsync<string>(result.Sql);
                    var obj = JsonConvert.DeserializeObject(json);
                    result.Data = JsonConvert.SerializeObject(obj, Formatting.Indented);
                }
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
            public string Sql { get; set; }
            public string Data { get; set; }
            public string Error { get; set; }
            public bool IsSuccess => Error == null;
        }
    }

    public class QueryModel
    {
        public string Query { get; set; }
        public string ParamsJson { get; set; }
    }
}
