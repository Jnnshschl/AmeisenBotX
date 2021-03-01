using AmeisenBotX.Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class BotUtilsTests
    {
        [TestMethod]
        public void BigValueToStringTest()
        {
            Assert.AreEqual("1", BotUtils.BigValueToString(1));
            Assert.AreEqual("10", BotUtils.BigValueToString(10));
            Assert.AreEqual("100", BotUtils.BigValueToString(100));
            Assert.AreEqual("1000", BotUtils.BigValueToString(1000));
            Assert.AreEqual("10000", BotUtils.BigValueToString(10000));
            Assert.AreEqual("100K", BotUtils.BigValueToString(100000));
            Assert.AreEqual("1000K", BotUtils.BigValueToString(1000000));
            Assert.AreEqual("10000K", BotUtils.BigValueToString(10000000));
            Assert.AreEqual("100M", BotUtils.BigValueToString(100000000));
            Assert.AreEqual("1000M", BotUtils.BigValueToString(1000000000));
        }

        [TestMethod]
        public void ByteArrayToStringTest()
        {
            byte[] bytes = new byte[] { 0x0, 0x35, 0xff };
            string s = BotUtils.ByteArrayToString(bytes);
            Assert.AreEqual("00 35 FF", s);
        }

        [TestMethod]
        public void CapitalizeTest()
        {
            string s = "lool";
            Assert.AreEqual("Lool", BotUtils.Capitalize(s));
        }

        [TestMethod]
        public void CleanStringTest()
        {
            string s = "lool\n\rhehexd";
            Assert.AreEqual("loolhehexd", BotUtils.CleanString(s));
        }

        [TestMethod]
        public void FastRandomStringOnlyLettersTest()
        {
            List<char> numbers = new() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            for (int i = 0; i < 16; ++i)
            {
                Assert.IsFalse(BotUtils.FastRandomStringOnlyLetters().Any(e => numbers.Contains(e)));
            }
        }

        [TestMethod]
        public void FastRandomStringTest()
        {
            for (int i = 0; i < 16; ++i)
            {
                Assert.IsTrue(BotUtils.FastRandomString().Length > 0);
            }
        }

        [TestMethod]
        public void GenerateUniqueStringTest()
        {
            for (int i = 1; i < 12; ++i)
            {
                Assert.IsTrue(BotUtils.GenerateUniqueString(i).Length > 0);
            }
        }

        [TestMethod]
        public void IsValidJsonTest()
        {
            string valid = "{ \"test\":\"lool\" }";
            string invalid = "{ \"test\":\"lool\"";

            string validA = "[{ \"test\":\"lool\" }]";
            string invalidA = "[{ \"test\":\"lool\"";

            Assert.IsTrue(BotUtils.IsValidJson(valid));
            Assert.IsFalse(BotUtils.IsValidJson(invalid));

            Assert.IsTrue(BotUtils.IsValidJson(validA));
            Assert.IsFalse(BotUtils.IsValidJson(invalidA));
        }

        // [TestMethod]
        // public void MoveAheadTest()
        // {
        //     Vector3 pos = new Vector3(0, 1, 0);
        //     Vector3 pos2 = new Vector3(0, 2, 0);
        //
        //     Vector3 aheadPos = BotUtils.MoveAhead(pos, pos2, 1);
        //     Assert.AreEqual(new Vector3(0, 3, 0), aheadPos);
        //
        //     aheadPos = BotUtils.MoveAhead(pos, pos2, -1);
        //     Assert.AreEqual(new Vector3(0, 1, 0), aheadPos);
        //
        //     aheadPos = BotUtils.MoveAhead(pos, (float)(Math.PI / 2.0), 1);
        //     Assert.AreEqual(new Vector3(0, 2, 0), aheadPos);
        //
        //     aheadPos = BotUtils.MoveAhead(pos, (float)(Math.PI / 2.0), -1);
        //     Assert.AreEqual(new Vector3(0, 0, 0), aheadPos);
        // }

        [TestMethod]
        public void ObfuscateLuaTest()
        {
            string x = BotUtils.FastRandomString();
            string sample = $"{{v:0}}={x}";
            (string, string) result = BotUtils.ObfuscateLua(sample);

            Assert.AreEqual(result.Item1, $"{result.Item2}={x}");
        }
    }
}