namespace GraphqlToTsql.Translator.Translator
{
    public class Sequence
    {
        private int _seq;

        public int Next()
        {
            return ++_seq;
        }
    }
}