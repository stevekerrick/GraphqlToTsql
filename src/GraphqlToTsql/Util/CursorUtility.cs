using GraphqlToTsql.Translator;
using System;
using System.Text;

namespace GraphqlToTsql.Util
{
    public static class CursorUtility
    {
        /// <summary>
        /// Create an obfuscated cursor, based on the the cursor "root" created in TSQL
        /// </summary>
        /// <param name="root">$"{idValue}|{dbTableName}"</param>
        /// <returns>Obfuscated cursor</returns>
        public static string CreateCursor(string root)
        {
            var rootBytes = Encoding.UTF8.GetBytes(root);
            var encodedRoot = Convert.ToBase64String(rootBytes);
            var hash = HashUtility.Hash(encodedRoot);
            return $"{encodedRoot}.{hash}";
        }

        public static int DecodeCursor(string dbTableName, string cursor)
        {
            if (string.IsNullOrEmpty(cursor))
            {
                throw new InvalidRequestException("Empty cursor is not allowed");
            }

            // Separate the cursor into the encoded root and hash
            var cursorParts = cursor.Split('.');
            if (cursorParts.Length != 2)
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }
            var encodedRoot = cursorParts[0];
            var actualHash = cursorParts[1];

            // Verify the hash
            var expectedHash = HashUtility.Hash(encodedRoot);
            if (actualHash != expectedHash)
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }

            // Decode the root. EncodedRoot is base64(root).
            string root;
            try
            {
                var rootBytes = Convert.FromBase64String(encodedRoot);
                root = Encoding.UTF8.GetString(rootBytes);
            }
            catch
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }

            // Separate the root into idValue and dbTableName
            var rootParts = root.Split('|');
            if (rootParts.Length != 2)
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }
            var idValue = rootParts[0];
            var actualDbTableName = rootParts[1];

            // Verify the DbTableName
            if (actualDbTableName != dbTableName)
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }

            return int.Parse(idValue);
        }
    }
}
