namespace GraphqlToSql.Transpiler.Transpiler
{
    public class TranslateResult
    {
        public bool IsSuccessful => string.IsNullOrWhiteSpace(ParseError);
        //public string ParseOutput { get; set; }
        public string ParseError { get; set; }
        public Query Query { get; set; }
    }
}
