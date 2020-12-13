using Dapper;
using GraphqlToTsql.Translator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Text.Json;
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
            //var result = await RunQuery(query);

            var result = new QueryResult
            {
                GraphQL = query.Query,
                Params = "todo",
                Sql = "todo",
                Data = "todo"
            };

            return new JsonResult(result);
        }

        private async Task<QueryResult> RunQuery(QueryModel query)
        {
            var result = new QueryResult { 
                GraphQL = query.Query,
                Params = "strange params"
            };

            // Build SQL command
            try
            {
                var translator = new GraphqlTranslator();
                var translateResult = translator.Translate(query.Query, null);
                if (!translateResult.IsSuccessful)
                {
                    result.Error = translateResult.ParseError;
                    return result;
                }
                result.Sql = translateResult.Query;
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
                    var obj = JsonSerializer.Deserialize<dynamic>(json);
                    result.Data = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
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
            public string GraphQL { get; set; }
            public string Params { get; set; }
            public string Sql { get; set; }
            public string Data { get; set; }
            public string Error { get; set; }
            public bool IsSuccess => Error == null;
        }
    }

    public class QueryModel
    {
        public string Query { get; set; }
    }
}
