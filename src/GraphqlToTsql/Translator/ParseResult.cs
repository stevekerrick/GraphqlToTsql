using System.Collections.Generic;

namespace GraphqlToTsql.Translator
{
    internal class ParseResult
    {
        public string OperationName { get; set; }
        public Dictionary<string, Term> Fragments { get; set; }
        public Term RootTerm { get; set; }
        public string ParseError { get; set; }
    }
}
