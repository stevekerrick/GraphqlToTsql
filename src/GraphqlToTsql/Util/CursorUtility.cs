using GraphqlToTsql.Translator;
using System;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Util
{
    internal static class CursorUtility
    {
        /// <summary>
        /// Func that generates a TSQL expression for the "decodedCursor".
        /// After the TSQL has been executed, the DataMutator will use the CreateCursor function below to
        /// transfrom the "decodedCursor" into the obfuscated (and hash-coded) cursor.
        /// </summary>
        public static string TsqlCursorDataFunc(ValueType valueType, string tableAlias, string dbTableName, string dbColumnName) =>
            $"CONCAT('{(int)valueType}', '|', {tableAlias}.[{dbColumnName}], '|', '{dbTableName}')";

        public static string CursorDataFunc(Value value, string dbTableName) => $"{(int)value.ValueType}|{value.RawValue}|{dbTableName}";

        public static string CreateCursor(Value value, string dbTableName) {
            return CreateCursor(CursorDataFunc(value, dbTableName));
        }

        /// <summary>
        /// Create an obfuscated cursor, based on the the "cursorData" created in TSQL.
        /// </summary>
        /// <param name="decodedCursor">
        ///   This is created in the TSQL (see Field.Cursor()).
        /// </param>
        /// <returns>Obfuscated cursor</returns>
        public static string CreateCursor(string decodedCursor)
        {
            if (string.IsNullOrEmpty(decodedCursor))
            {
                throw new Exception("Empty cursor data");
            }

            var encodedCursorData = Base64Utility.Encode(decodedCursor);
            var hash = HashUtility.Hash(encodedCursorData);
            return $"{encodedCursorData}.{hash}";
        }

        public static CursorData DecodeCursor(string cursor, string dbTableName)
        {
            if (string.IsNullOrEmpty(cursor))
            {
                throw new InvalidRequestException(ErrorCode.V28, "Empty cursor is not allowed");
            }

            // Separate the cursor into the encoded cursor data and hash
            var cursorParts = cursor.Split('.');
            if (cursorParts.Length != 2)
            {
                throw new InvalidRequestException(ErrorCode.V28, $"Cursor is invalid: {cursor}");
            }
            var encodedCursorData = cursorParts[0];
            var actualHash = cursorParts[1];

            // Verify the hash
            var expectedHash = HashUtility.Hash(encodedCursorData);
            if (actualHash != expectedHash)
            {
                throw new InvalidRequestException(ErrorCode.V28, $"Cursor is invalid: {cursor}");
            }

            // Decode the CursorData. EncodedCursorData is base64(CursorData).
            string decodedCursor;
            try
            {
                decodedCursor = Base64Utility.Decode(encodedCursorData);
            }
            catch
            {
                throw new InvalidRequestException(ErrorCode.V28, $"Cursor is invalid: {cursor}");
            }

            // Separate the decodedCursor into valueType, idValue, and dbTableName
            var cursorDataParts = decodedCursor.Split('|');
            if (cursorDataParts.Length != 3)
            {
                throw new InvalidRequestException(ErrorCode.V28, $"Cursor is invalid: {cursor}");
            }
            var valueType = (ValueType)int.Parse(cursorDataParts[0]);
            var idValue = cursorDataParts[1];
            var actualDbTableName = cursorDataParts[2];

            // Verify the DbTableName
            if (actualDbTableName != dbTableName)
            {
                throw new InvalidRequestException(ErrorCode.V28, $"Cursor is invalid: {cursor}");
            }

            // Return the value
            return new CursorData
            {
                DbTableName = actualDbTableName,
                Value = Value.FromStringValue(valueType, idValue)
            };
        }
    }

    internal class CursorData
    {
        public string DbTableName { get; set; }
        public Value Value { get; set; }
    }
}
