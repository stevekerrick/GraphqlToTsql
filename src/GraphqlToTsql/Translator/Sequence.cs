namespace GraphqlToTsql.Translator
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