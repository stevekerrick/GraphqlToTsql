using GraphqlToTsql.Translator;
using System.Collections.Generic;

namespace GraphqlToTsql
{
    /// <summary>
    /// Result of the TranslateToTsql action
    /// </summary>
    public class TsqlResult
    {
        /// <summary>
        /// The TSQL command that was translated from the GraphQL
        /// </summary>
        public string Tsql { get; set; }

        /// <summary>
        /// The translation produces parameterized TSQL. The TsqlParameters dictionary are the parameters
        /// to send to the database along with the TSQL.
        /// </summary>
        public Dictionary<string, object> TsqlParameters { get; set; }

        /// <summary>
        /// Error that occurred during translation. This is always a problem in the source GraphQL and its parameters.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Error code.
        /// </summary>
        public ErrorCode ErrorCode { get; set; }
    }
}
