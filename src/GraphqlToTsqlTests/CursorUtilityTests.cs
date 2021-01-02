using AutoFixture;
using GraphqlToTsql.Translator;
using GraphqlToTsql.Util;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using System;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class CursorUtilityTests
    {
        private Fixture _fixture = new Fixture();

        [Test]
        public void CreateCursorTest()
        {
            var parts = _fixture.Create<CursorParts>();
            var cursor = CursorUtility.CreateCursor(parts.CursorData);

            Console.WriteLine(cursor);
            Assert.IsNotNull(cursor);
        }

        [Test]
        public void CreateAndDecodeCursorTest()
        {
            var parts = _fixture.Create<CursorParts>();
            var cursor = CursorUtility.CreateCursor(parts.CursorData);

            var decodedIdValue = CursorUtility.DecodeCursor(parts.DbTableName, cursor);
            Assert.AreEqual(parts.IdValue, decodedIdValue);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("abcd.efgh.ijkl")]
        public void DecodeIllFormedCursorTest(string cursor)
        {
            var dbTableName = _fixture.Create<string>();
            Assert.Throws<InvalidRequestException>(() => CursorUtility.DecodeCursor(dbTableName, cursor));
        }

        [TestCaseSource(nameof(CorruptorFuncs))]
        public void CorruptedCursorTest(Func<string, CursorParts, string> corruptorFunc)
        {
            // Create an actual cursor
            var parts = _fixture.Create<CursorParts>();
            var cursor = CursorUtility.CreateCursor(parts.CursorData);

            // Corrupt the cursor and try to decode it
            var corrputedCursor = corruptorFunc(cursor, parts);
            Assert.Throws<InvalidRequestException>(() => CursorUtility.DecodeCursor(parts.DbTableName, corrputedCursor));
        }

        static Func<string, CursorParts, string>[] CorruptorFuncs =
        {
            NullCursor,
            EmptyStringCursor,
            ChangeHash,
            ChangeIdValue,
            InvalidBase64Chars
        };

        static string NullCursor(string cursor, CursorParts parts) => null;

        static string EmptyStringCursor(string cursor, CursorParts parts) => "";

        static string ChangeHash(string cursor, CursorParts parts)
        {
            var fakeHash = HashUtility.Hash("fake");
            var p = cursor.IndexOf('.');
            return cursor.Substring(0, p) + "." + fakeHash;
        }

        static string ChangeIdValue(string cursor, CursorParts parts)
        {
            var newCursorData = $"{parts.IdValue + 1}|{parts.DbTableName}";
            var newCursor = CursorUtility.CreateCursor(newCursorData);

            var pOld = cursor.IndexOf('.');
            var pNew = newCursor.IndexOf('.');

            return newCursor.Substring(0, pNew) + cursor.Substring(pOld);
        }

        static string InvalidBase64Chars(string cursor, CursorParts parts)
        {
            return '!' + cursor.Substring(1);
        }
    }

    public class CursorParts
    {
        public int IdValue { get; set; }
        public string DbTableName { get; set; }
        public string CursorData => $"{IdValue}|{DbTableName}";
    }
}
