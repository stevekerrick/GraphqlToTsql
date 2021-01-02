namespace GraphqlToTsql.Translator
{
    public class AliasSequence
    {
        private int _seq;

        public string Next()
        {
            return $"t{++_seq}";
        }
    }
}