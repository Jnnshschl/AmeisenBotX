using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class BotMathTests
    {
        [TestMethod]
        public void AngleCalculationTest()
        {
            Vector3 middlePos = new Vector3(0, 0, 0);
            Vector3 topPos = new Vector3(0, 4, 0);
            Vector3 bottomPos = new Vector3(0, -4, 0);
            Vector3 leftPos = new Vector3(-4, 0, 0);
            Vector3 rightPos = new Vector3(4, 0, 0);

            float facingAngle = BotMath.GetFacingAngle2D(middlePos, rightPos);
            Assert.AreEqual(0f, Math.Round(facingAngle, 4));

            facingAngle = BotMath.GetFacingAngle2D(middlePos, topPos);
            Assert.AreEqual(Math.Round(Math.PI * 0.5, 4), Math.Round(facingAngle, 4));

            facingAngle = BotMath.GetFacingAngle2D(middlePos, bottomPos);
            Assert.AreEqual(Math.Round(Math.PI * 1.5, 4), Math.Round(facingAngle, 4));

            facingAngle = BotMath.GetFacingAngle2D(middlePos, leftPos);
            Assert.AreEqual(Math.Round(Math.PI, 4), Math.Round(facingAngle, 4));
        }

        [TestMethod]
        public void CalculatePositionBehindTest()
        {
            Vector3 topPos = new Vector3(0, 4, 0);
            Vector3 middlePos = new Vector3(0, 0, 0);

            float facingAngle = BotMath.GetFacingAngle2D(topPos, middlePos);
            Vector3 calculatedPos = BotMath.CalculatePositionBehind(topPos, facingAngle, 2.0);

            Vector3 expectedPos = new Vector3(0, 6, 0);
            Assert.AreEqual(expectedPos, calculatedPos);
        }

        [TestMethod]
        public void CapVector3Test()
        {
            Vector3 vector = new Vector3(8, 8, 8);
            Assert.AreEqual(new Vector3(4, 4, 4), BotMath.CapVector3(vector, 4));
        }
    }
}