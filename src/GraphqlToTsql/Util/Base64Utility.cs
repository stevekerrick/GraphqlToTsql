using System;
using System.Text;

namespace GraphqlToTsql.Util
{
    public static class Base64Utility
    {
        public static string Encode(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }

        public static string Decode(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
