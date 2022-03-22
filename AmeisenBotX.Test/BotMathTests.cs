using AmeisenBotX.Common.Math;
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
            Vector3 middlePos = new(0, 0, 0);
            Vector3 topPos = new(0, 4, 0);
            Vector3 bottomPos = new(0, -4, 0);
            Vector3 leftPos = new(-4, 0, 0);
            Vector3 rightPos = new(4, 0, 0);

            float facingAngle = BotMath.GetFacingAngle(middlePos, rightPos);
            Assert.AreEqual(0f, MathF.Round(facingAngle, 4));

            facingAngle = BotMath.GetFacingAngle(middlePos, topPos);
            Assert.AreEqual(MathF.Round(MathF.PI * 0.5f, 4), MathF.Round(facingAngle, 4));

            facingAngle = BotMath.GetFacingAngle(middlePos, bottomPos);
            Assert.AreEqual(MathF.Round(MathF.PI * 1.5f, 4), MathF.Round(facingAngle, 4));

            facingAngle = BotMath.GetFacingAngle(middlePos, leftPos);
            Assert.AreEqual(MathF.Round(MathF.PI, 4), MathF.Round(facingAngle, 4));
        }

        [TestMethod]
        public void ClampAnglesTest()
        {
            float clampedA = BotMath.ClampAngle(9.0f);
            float clampedB = BotMath.ClampAngle(-3.0f);

            Assert.IsTrue(clampedA >= 0.0f && clampedA <= MathF.Tau);
            Assert.IsTrue(clampedB >= 0.0f && clampedB <= MathF.Tau);
        }

        [TestMethod]
        public void IsFacingTest()
        {
            Vector3 middlePos = new(0, 0, 0);

            Vector3 topPos = new(0, 4, 0);
            Vector3 leftPos = new(-4, 0, 0);
            Vector3 bottomPos = new(0, -4, 0);
            Vector3 rightPos = new(4, 0, 0);

            float rotation = MathF.PI / 2.0f;

            Assert.IsTrue(BotMath.IsFacing(middlePos, rotation, topPos));

            Assert.IsFalse(BotMath.IsFacing(middlePos, rotation, rightPos));
            Assert.IsFalse(BotMath.IsFacing(middlePos, rotation, bottomPos));
            Assert.IsFalse(BotMath.IsFacing(middlePos, rotation, leftPos));
        }
    }
}