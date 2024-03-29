﻿using DemoEntities;
using GraphqlToTsql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphqlController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IGraphqlActions _graphqlActions;

        public GraphqlController(
            IConfiguration configuration,
            IGraphqlActions graphqlActions)
        {
            _configuration = configuration; ;
            _graphqlActions = graphqlActions;
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
                response = new { result.Errors, ErrorCode = result.ErrorCode.ToString() };
            }
            else
            {
                response = new { result.Data };
            }

            return new JsonResult(response);
        }

        [Route("KeepAlive")]
        [HttpGet]
        public void KeepAlive()
        {
            var settings = new GraphqlActionSettings
            {
                AllowIntrospection = true,
                EntityList = DemoEntityList.All()
            };

            _graphqlActions.TranslateToTsql (
                "{ __schema { types { name fields { name } } } }",
                new Dictionary<string, object>(),
                settings
            );
        }

        private async Task<QueryResponse> RunQuery(QueryRequest query)
        {
            var graphql = query.Query;
            var graphqlParameters = query.Variables ?? new Dictionary<string, object>();

            var connectionString = _configuration.GetConnectionString("DemoDB");
            if (string.IsNullOrEmpty(connectionString) || connectionString == "set in azure")
            {
                throw new Exception("Database connection string is not set");
            }

            var settings = new GraphqlActionSettings
            {
                AllowIntrospection = true,
                EntityList = DemoEntityList.All(),
                ConnectionString = _configuration.GetConnectionString("DemoDB")
            };

            var queryResult = await _graphqlActions.TranslateAndRunQuery(graphql, graphqlParameters, settings);

            var errors =
                queryResult.TranslationError != null ? new[] { queryResult.TranslationError }
                : queryResult.DbError != null ? new[] { queryResult.DbError }
                : null;

            return new QueryResponse
            {
                Data = Deserialize(queryResult.DataJson),
                Errors = errors,
                Tsql = queryResult.Tsql,
                TsqlParameters = queryResult.TsqlParameters,
                Statistics = queryResult.Statistics,
                ErrorCode = queryResult.ErrorCode.ToString()
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

        // The GraphQL variable values
        public Dictionary<string, object> Variables { get; set; }
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
        public string ErrorCode { get; set; }
    }
}
