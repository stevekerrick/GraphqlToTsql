using GraphqlToTsql.Translator;
using System.Collections.Generic;

namespace GraphqlToTsql
{
    /// <summary>
    /// Result of the TranslateAndRunQuery action.
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        /// The TSQL command that was translated from the GraphQL.
        /// </summary>
        public string Tsql { get; set; }

        /// <summary>
        /// The translation produces parameterized TSQL. The TsqlParameters dictionary are the parameters
        /// to send to the database along with the TSQL.
        /// </summary>
        public Dictionary<string, object> TsqlParameters { get; set; }

        /// <summary>
        /// The result of the query, in JSON format.
        /// </summary>
        public string DataJson { get; set; }

        /// <summary>
        /// Error that occurred trying to translate the GraphQL into TSQL.
        /// This is always a problem in the source GraphQL and its parameters.
        /// </summary>
        public string TranslationError { get; set; }

        /// <summary>
        /// Error that occurred executing the TSQL command. Can by caused by incorrect entity mapping, or database error.
        /// </summary>
        public string DbError { get; set; }

        /// <summary>
        /// Error code.
        /// </summary>
        public ErrorCode ErrorCode { get; set; }

        /// <summary>
        /// Was the action successful? If true then read the results are in DataJson. If false then look at
        /// TranslationError and DbError.
        /// </summary>
        public bool IsSuccessful => string.IsNullOrWhiteSpace(TranslationError) || string.IsNullOrWhiteSpace(DbError);

        /// <summary>
        /// Statistics about the action.
        /// <list type="bullet">
        /// <item><description>Time to parse the GraphQL (ms)</description></item>
        /// <item><description>Time to create TSQL (ms)</description></item>
        /// <item><description>Total time to execute the TSQL (ms)</description></item>
        /// <item><description>Time spent by SQL Server (ms)</description></item>
        /// <item><description>Size of the TSQL query (chars)</description></item>
        /// <item><description>Size of the resulting JSON (chars)</description></item>
        /// <item><description>Time spent post-processing the resulting JSON (ms)</description></item>
        /// </list>
        /// </summary>
        public List<Statistic> Statistics { get; set; }
    }

    public class Statistic
    {
        public string Name { get; set; }
        public long? Value { get; set; }

        public Statistic(string name, long? value)
        {
            Name = name;
            Value = value;
        }
    }
}
