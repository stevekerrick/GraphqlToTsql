namespace GraphqlToTsql.Translator
{
    internal class AliasSequence
    {
        private int _seq;

        public string Next()
        {
            return $"t{++_seq}";
        }
    }
}