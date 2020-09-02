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

        // [TestMethod]
        // public void CalculatePositionAroundTest()
        // {
        //     Vector3 middlePos = new Vector3(0, 0, 0);
        //
        //     Vector3 topPos = new Vector3(0, 4, 0);
        //     Vector3 leftPos = new Vector3(-4, 0, 0);
        //     Vector3 bottomPos = new Vector3(0, -4, 0);
        //     Vector3 rightPos = new Vector3(4, 0, 0);
        //
        //     Assert.AreEqual(rightPos, BotMath.CalculatePositionAround(middlePos, 0f, 0f, 4));
        //     Assert.AreEqual(topPos, BotMath.CalculatePositionAround(middlePos, 0f, (float)Math.PI / 2f, 4));
        //     Assert.AreEqual(leftPos, BotMath.CalculatePositionAround(middlePos, 0f, (float)Math.PI, 4));
        //     Assert.AreEqual(bottomPos, BotMath.CalculatePositionAround(middlePos, 0f, (float)Math.PI * 1.5f, 4));
        //     Assert.AreEqual(rightPos, BotMath.CalculatePositionAround(middlePos, 0f, (float)Math.PI * 2f, 4));
        // }
        //
        // [TestMethod]
        // public void CalculatePositionBehindTest()
        // {
        //     Vector3 topPos = new Vector3(0, 4, 0);
        //     Vector3 middlePos = new Vector3(0, 0, 0);
        //
        //     float facingAngle = BotMath.GetFacingAngle2D(topPos, middlePos);
        //     Vector3 calculatedPos = BotMath.CalculatePositionBehind(topPos, facingAngle, (float)2.0);
        //
        //     Vector3 expectedPos = new Vector3(0, 6, 0);
        //     Assert.AreEqual(expectedPos, calculatedPos);
        // }

        [TestMethod]
        public void ClampAnglesTest()
        {
            float clampedA = BotMath.ClampAngles(9f);
            float clampedB = BotMath.ClampAngles(-3f);

            Assert.IsTrue(clampedA >= 0 && clampedA <= Math.PI * 2);
            Assert.IsTrue(clampedB >= 0 && clampedB <= Math.PI * 2);
        }

        [TestMethod]
        public void IsFacingTest()
        {
            Vector3 middlePos = new Vector3(0, 0, 0);

            Vector3 topPos = new Vector3(0, 4, 0);
            Vector3 leftPos = new Vector3(-4, 0, 0);
            Vector3 bottomPos = new Vector3(0, -4, 0);
            Vector3 rightPos = new Vector3(4, 0, 0);

            float rotation = (float)Math.PI / 2f;

            Assert.IsTrue(BotMath.IsFacing(middlePos, rotation, topPos));

            Assert.IsFalse(BotMath.IsFacing(middlePos, rotation, rightPos));
            Assert.IsFalse(BotMath.IsFacing(middlePos, rotation, bottomPos));
            Assert.IsFalse(BotMath.IsFacing(middlePos, rotation, leftPos));
        }
    }
}