using AmeisenBotX.Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        public void ObfuscateLuaTest()
        {
            string x = BotUtils.FastRandomString();
            string sample = $"{{v:0}}={x}";
            (string, string) result = BotUtils.ObfuscateLua(sample);

            Assert.AreEqual(result.Item1, $"{result.Item2}={x}");
        }
    }
}