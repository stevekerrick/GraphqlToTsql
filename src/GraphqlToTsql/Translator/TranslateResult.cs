namespace GraphqlToTsql.Translator
{
    public class TranslateResult
    {
        public bool IsSuccessful => string.IsNullOrWhiteSpace(ParseError);
        public string ParseError { get; set; }
        public string Tsql { get; set; }
    }
}
