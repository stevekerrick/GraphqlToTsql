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
        private AutoMocker _mocks = new AutoMocker(MockBehavior.Strict);
        private static IHashUtility _hashUtility = new HashUtility();

        [Test]
        public void CreateCursorTest()
        {
            var parts = GetCursorPartsAndSetupHashUtility();

            var cursorUtility = _mocks.CreateInstance<CursorUtility>();
            var cursor = cursorUtility.CreateCursor(parts.DbTableName, parts.IdValue);

            Console.WriteLine(cursor);
            Assert.IsTrue(cursor.EndsWith($".{parts.Hash}"));
        }

        [Test]
        public void CreateAndDecodeCursorTest()
        {
            var parts = GetCursorPartsAndSetupHashUtility();

            var cursorUtility = _mocks.CreateInstance<CursorUtility>();
            var cursor = cursorUtility.CreateCursor(parts.DbTableName, parts.IdValue);

            var decodedIdValue = cursorUtility.DecodeCursor(parts.DbTableName, cursor);
            Assert.AreEqual(parts.IdValue, decodedIdValue);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("abcd.efgh.ijkl")]
        [TestCase("")]
        public void DecodeIllFormedCursorTest(string cursor)
        {
            var cursorUtility = _mocks.CreateInstance<CursorUtility>();
            var dbTableName = _fixture.Create<string>();
            Assert.Throws<InvalidRequestException>(() => cursorUtility.DecodeCursor(dbTableName, cursor));
        }

        [TestCaseSource(nameof(CorruptorFuncs))]
        public void CorruptedCursorTest(Func<string, CursorParts, string> corruptorFunc)
        {
            // For this test, use the real HashUtility
            var cursorUtility = new CursorUtility(_hashUtility);

            // Create an actual cursor
            var parts = GetCursorPartsAndSetupHashUtility();
            var cursor = cursorUtility.CreateCursor(parts.DbTableName, parts.IdValue);
            Console.WriteLine(cursor);

            // Corrupt the cursor and try to decode it
            var corrputedCursor = corruptorFunc(cursor, parts);
            Assert.Throws<InvalidRequestException>(() => cursorUtility.DecodeCursor(parts.DbTableName, corrputedCursor));
        }

        private CursorParts GetCursorPartsAndSetupHashUtility()
        {
            var parts = _fixture.Create<CursorParts>();

            _mocks.GetMock<IHashUtility>()
                .Setup(_ => _.Hash(It.IsAny<string>()))
                .Returns(parts.Hash);

            return parts;
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
            var fakeHash = _hashUtility.Hash("fake");
            var p = cursor.IndexOf('.');
            return cursor.Substring(0, p) + "." + fakeHash;
        }

        static string ChangeIdValue(string cursor, CursorParts parts)
        {
            var cursorUtility = new CursorUtility(_hashUtility);
            var newCursor = cursorUtility.CreateCursor(parts.DbTableName, parts.IdValue + 1);

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
        public string Hash { get; set; }
    }
}
