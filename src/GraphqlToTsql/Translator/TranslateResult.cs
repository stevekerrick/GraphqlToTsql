using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    // TODO: Rename (and move codefile?)
    public class TranslateResult
    {
        public string Tsql { get; set; }
        public Dictionary<string, object> TsqlParameters { get; set; }
        public string DataJson { get; set; }
        public string ParseError { get; set; }
        public string DbError { get; set; }
        public bool IsSuccessful => string.IsNullOrWhiteSpace(ParseError) || string.IsNullOrWhiteSpace(DbError);
    }
}
