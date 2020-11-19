namespace GraphqlToTsql.Translator.Translator
{
    public class TranslateResult
    {
        public bool IsSuccessful => string.IsNullOrWhiteSpace(ParseError);
        public string ParseError { get; set; }
        public string Query { get; set; }
    }
}
