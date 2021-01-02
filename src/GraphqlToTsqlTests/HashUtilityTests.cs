using GraphqlToTsql.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class HashUtilityTests
    {
        [Test]
        public void WellKnownHashTest()
        {
            var hash = HashUtility.Hash("abcdefg");
            Assert.AreEqual("81534fe", hash, "Hash produced unexpected value");
        }

        [Test]
        public void HashAvoidsCollisionsTest()
        {
            var dict = new Dictionary<string, string>();

            for(int i=0; i<1000; i++)
            {
                var str = Guid.NewGuid().ToString();
                var hash = HashUtility.Hash(str);
                if (dict.ContainsKey(hash))
                {
                    Assert.Fail($"Hash collision: [{dict[hash]}] and [{str}] both have hash [{hash}]");
                }

                dict[hash] = str;
            }
        }
    }
}
