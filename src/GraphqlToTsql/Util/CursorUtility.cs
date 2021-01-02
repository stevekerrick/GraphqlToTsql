using GraphqlToTsql.Translator;
using System;
using System.Text;

namespace GraphqlToTsql.Util
{
    public interface ICursorUtility
    {
        string CreateCursor(string dbTableName, int idValue);
        int DecodeCursor(string dbTableName, string cursor);
    }

    public class CursorUtility : ICursorUtility
    {
        private readonly IHashUtility _hashUtility;

        public CursorUtility(IHashUtility hashUtility)
        {
            _hashUtility = hashUtility;
        }

        public string CreateCursor(string dbTableName, int idValue)
        {
            var root = $"{idValue}|{dbTableName}";
            var rootBytes = Encoding.UTF8.GetBytes(root);
            var encodedRoot = Convert.ToBase64String(rootBytes);
            var hash = _hashUtility.Hash(encodedRoot);
            return $"{encodedRoot}.{hash}";
        }

        public int DecodeCursor(string dbTableName, string cursor)
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
            var expectedHash = _hashUtility.Hash(encodedRoot);
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
            catch (Exception e)
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
