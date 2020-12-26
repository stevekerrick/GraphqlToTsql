using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    public class ParseResult
    {
        public string OperationName { get; set; }
        public Dictionary<string, Term> Fragments { get; set; }
        public Term TopTerm { get; set; }
    }
}
