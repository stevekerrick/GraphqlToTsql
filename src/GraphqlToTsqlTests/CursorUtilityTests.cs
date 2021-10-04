using AutoFixture;
using GraphqlToTsql.Translator;
using GraphqlToTsql.Util;
using NUnit.Framework;
using System;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class CursorUtilityTests
    {
        private Fixture _fixture = new Fixture();

        [Test]
        public void CreateAndDecodeIntCursorTest()
        {
            var value = RandomIntValue();
            var dbTableName = _fixture.Create<string>();

            var cursor = CursorUtility.CreateCursor(value, dbTableName);
            var cursorData = CursorUtility.DecodeCursor(cursor, dbTableName);

            Assert.AreEqual(value.ValueType, cursorData.Value.ValueType);
            Assert.AreEqual(value.RawValue, cursorData.Value.RawValue);
            Assert.AreEqual(dbTableName, cursorData.DbTableName);
        }

        [Test]
        public void CreateAndDecodeStringCursorTest()
        {
            var value = RandomStringValue();
            var dbTableName = _fixture.Create<string>();

            var cursor = CursorUtility.CreateCursor(value, dbTableName);
            var cursorData = CursorUtility.DecodeCursor(cursor, dbTableName);

            Assert.AreEqual(value.ValueType, cursorData.Value.ValueType);
            Assert.AreEqual(value.RawValue, cursorData.Value.RawValue);
            Assert.AreEqual(dbTableName, cursorData.DbTableName);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("abcd.efgh.ijkl")]
        public void DecodeIllFormedCursorTest(string cursor)
        {
            var dbTableName = _fixture.Create<string>();
            Assert.Throws<InvalidRequestException>(() => CursorUtility.DecodeCursor(cursor, dbTableName));
        }

        [TestCaseSource(nameof(CorruptorFuncs))]
        public void CorruptedCursorTest(Func<string, string> corruptorFunc)
        {
            // Create an actual cursor
            var value = RandomIntValue();
            var dbTableName = _fixture.Create<string>();
            var cursor = CursorUtility.CreateCursor(value, dbTableName);

            // Corrupt the cursor and try to decode it
            var corrputedCursor = corruptorFunc(cursor);
            Assert.Throws<InvalidRequestException>(() => CursorUtility.DecodeCursor(corrputedCursor, dbTableName));
        }

        static Func<string, string>[] CorruptorFuncs =
        {
            NullCursor,
            EmptyStringCursor,
            ChangeHash,
            ChangeIdValue,
            InvalidBase64Chars
        };

        static string NullCursor(string cursor) => null;

        static string EmptyStringCursor(string cursor) => "";

        static string ChangeHash(string cursor)
        {
            var fakeHash = HashUtility.Hash("fake");
            var p = cursor.IndexOf('.');
            return cursor.Substring(0, p) + "." + fakeHash;
        }

        static string ChangeIdValue(string cursor)
        {
            var parts1 = cursor.Split('.');
            var decodedCursor = Base64Utility.Decode(parts1[0]);
            var parts2 = decodedCursor.Split('|');

            parts2[1] = "12345";

            var newDecodedCursor = string.Join('|', parts2);
            parts1[0] = Base64Utility.Encode(newDecodedCursor);
            return string.Join('.', parts1);
        }

        static string InvalidBase64Chars(string cursor)
        {
            return '!' + cursor.Substring(1);
        }
 
        Value RandomIntValue()
        {
            return new Value(ValueType.Int, _fixture.Create<int>());
        }

        Value RandomStringValue()
        {
            return new Value(ValueType.String, _fixture.Create<string>());
        }
    }
}
