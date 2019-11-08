using AmeisenBotX.Core.Common;
using AmeisenBotX.Pathfinding.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class BotMathTests
    {
        [TestMethod]
        public void CapVector3Test()
        {
            Vector3 vector = new Vector3(8, 8, 8);
            Assert.AreEqual(new Vector3(4, 4, 4), BotMath.CapVector3(vector, 4));
        }
    }
}
