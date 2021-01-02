using GraphqlToTsql.Translator;
using System;
using System.Text;

namespace GraphqlToTsql.Util
{
    public static class CursorUtility
    {
        /// <summary>
        /// Create an obfuscated cursor, based on the the "cursorData" created in TSQL
        /// </summary>
        /// <param name="cursorData">$"{idValue}|{dbTableName}"</param>
        /// <returns>Obfuscated cursor</returns>
        public static string CreateCursor(string cursorData)
        {
            if (cursorData == null)
            {
                return null;
            }

            var cursorDataBytes = Encoding.UTF8.GetBytes(cursorData);
            var encodedCursorData = Convert.ToBase64String(cursorDataBytes);
            var hash = HashUtility.Hash(encodedCursorData);
            return $"{encodedCursorData}.{hash}";
        }

        public static int DecodeCursor(string dbTableName, string cursor)
        {
            if (string.IsNullOrEmpty(cursor))
            {
                throw new InvalidRequestException("Empty cursor is not allowed");
            }

            // Separate the cursor into the encoded cursor data and hash
            var cursorParts = cursor.Split('.');
            if (cursorParts.Length != 2)
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }
            var encodedCursorData = cursorParts[0];
            var actualHash = cursorParts[1];

            // Verify the hash
            var expectedHash = HashUtility.Hash(encodedCursorData);
            if (actualHash != expectedHash)
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }

            // Decode the CursorData. EncodedCursorData is base64(CursorData).
            string cursorData;
            try
            {
                var cursorDataBytes = Convert.FromBase64String(encodedCursorData);
                cursorData = Encoding.UTF8.GetString(cursorDataBytes);
            }
            catch
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }

            // Separate the cursorData into idValue and dbTableName
            var cursorDataParts = cursorData.Split('|');
            if (cursorDataParts.Length != 2)
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }
            var idValue = cursorDataParts[0];
            var actualDbTableName = cursorDataParts[1];

            // Verify the DbTableName
            if (actualDbTableName != dbTableName)
            {
                throw new InvalidRequestException($"Cursor is invalid: {cursor}");
            }

            return int.Parse(idValue);
        }
    }
}
