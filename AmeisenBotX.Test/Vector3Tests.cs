using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class Vector3Tests
    {
        [TestMethod]
        public void Vector3AddTest()
        {
            Vector3 a = new(1, 1, 1);
            Vector3 b = new(1, 1, 1);

            Assert.AreEqual(new(2, 2, 2), a + 1f);
            Assert.AreEqual(new(2, 2, 2), a + b);

            Assert.AreEqual(new(1, 1, 1), a);
            Assert.AreEqual(new(1, 1, 1), b);

            a.Add(b);

            Assert.AreEqual(new(2, 2, 2), a);
            Assert.AreEqual(new(1, 1, 1), b);

            a.Add(1f);

            Assert.AreEqual(new(3, 3, 3), a);
        }

        [TestMethod]
        public void Vector3ComparisonTest()
        {
            Vector3 a = new(2, 2, 2);
            Vector3 b = new(2, 2, 2);

            if (a != b)
            {
                Assert.Fail();
            }

            b = new(2, 1, 2);

            if (a == b)
            {
                Assert.Fail();
            }

            if (a > b)
            {
                Assert.Fail();
            }

            b = new(2, 2, 2);

            if (!a.Equals(b))
            {
                Assert.Fail();
            }

            if (a < b || a > b)
            {
                Assert.Fail();
            }

            b = new(2, 3, 2);

            if (a < b)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void Vector3DistanceTest()
        {
            Vector3 a = new(2, 2, 2);
            Vector3 b = new(2, 2, 2);

            if (a.GetDistance(b) != 0.0)
            {
                Assert.Fail();
            }

            b = new(2, 2, 1);

            if (a.GetDistance(b) != 1.0)
            {
                Assert.Fail();
            }

            if (a.GetDistance2D(b) != 0.0)
            {
                Assert.Fail();
            }

            b = new(2, 2, 0);

            if (a.GetDistance(b) != 2.0)
            {
                Assert.Fail();
            }

            if (a.GetDistance2D(b) != 0.0)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void Vector3DivideTest()
        {
            Vector3 a = new(2, 2, 2);
            Vector3 b = new(2, 2, 2);

            Assert.AreEqual(new(1, 1, 1), a / 2f);
            Assert.AreEqual(new(1, 1, 1), a / b);

            Assert.AreEqual(new(2, 2, 2), a);
            Assert.AreEqual(new(2, 2, 2), b);

            a.Divide(b);

            Assert.AreEqual(new(1, 1, 1), a);
            Assert.AreEqual(new(2, 2, 2), b);

            a.Divide(2f);

            Assert.AreEqual(new(0.5f, 0.5f, 0.5f), a);
        }

        [TestMethod]
        public void Vector3LimitTest()
        {
            Vector3 a = new(2, 2, 2);
            Vector3 b = new(-2, -2, -2);

            a.Limit(1);

            if (a.X > 1 || a.Y > 1 || a.Z > 1)
            {
                Assert.Fail();
            }

            b.Limit(-1);

            if (b.X < -1 || b.Y < -1 || b.Z < -1)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void Vector3MagnitudeTest()
        {
            Vector3 a = new(2, 0, 2000);
            Vector3 b = new(-2, 0, -2000);

            Assert.AreEqual(2f, a.GetMagnitude2D());
            Assert.AreEqual(2f, b.GetMagnitude2D());

            a = new(0, 0, 2);
            b = new(0, 0, -2);

            Assert.AreEqual(2f, a.GetMagnitude());
            Assert.AreEqual(2f, b.GetMagnitude());
        }

        [TestMethod]
        public void Vector3MiscTest()
        {
            Vector3 a = new(2, 1, 2000);
            Vector3 b = new(2, 1, 2000);

            if (a.GetHashCode() != b.GetHashCode())
            {
                Assert.Fail();
            }

            float[] x = new float[] { 2f, 1f, 2000f };
            float[] y = a.ToArray();

            Assert.AreEqual(x[0], y[0]);
            Assert.AreEqual(x[1], y[1]);
            Assert.AreEqual(x[2], y[2]);

            Assert.AreEqual(a, Vector3.FromArray(y));

            Vector3 c = new Vector3(a);

            Assert.AreEqual(a, c);
        }

        [TestMethod]
        public void Vector3MultiplyTest()
        {
            Vector3 a = new(2, 2, 2);
            Vector3 b = new(2, 2, 2);

            Assert.AreEqual(new(4, 4, 4), a * 2f);

            Assert.AreEqual(new(4, 4, 4), a * b);

            Assert.AreEqual(new(2, 2, 2), a);
            Assert.AreEqual(new(2, 2, 2), b);

            a.Multiply(b);

            Assert.AreEqual(new(4, 4, 4), a);
            Assert.AreEqual(new(2, 2, 2), b);

            a.Multiply(2f);

            Assert.AreEqual(new(8, 8, 8), a);
        }

        [TestMethod]
        public void Vector3NormalizingTest()
        {
            Vector3 a = new(2, 1, 2000);
            Vector3 b = new(-2, -1, -2000);

            a.Normalize2D();

            Assert.AreEqual(0.8944, Math.Round(a.X, 4));
            Assert.AreEqual(0.4472, Math.Round(a.Y, 4));
            Assert.AreEqual(2000f, a.Z);

            b.Normalize2D();

            Assert.AreEqual(-0.8944, Math.Round(b.X, 4));
            Assert.AreEqual(-0.4472, Math.Round(b.Y, 4));
            Assert.AreEqual(-2000f, b.Z);

            a = new(1, 2, 4);
            b = new(-1, -2, -4);

            a.Normalize();

            Assert.AreEqual(0.2182, Math.Round(a.X, 4));
            Assert.AreEqual(0.4364, Math.Round(a.Y, 4));
            Assert.AreEqual(0.8729, Math.Round(a.Z, 4));

            b.Normalize();

            Assert.AreEqual(-0.2182, Math.Round(b.X, 4));
            Assert.AreEqual(-0.4364, Math.Round(b.Y, 4));
            Assert.AreEqual(-0.8729, Math.Round(b.Z, 4));
        }

        [TestMethod]
        public void Vector3RotationTest()
        {
            Vector3 a = new(1, 0, 0);

            a.Rotate(180);

            Assert.AreEqual(-1f, a.X);

            a = new(1, 0, 0);
            a.RotateRadians(MathF.PI);

            Assert.AreEqual(-1f, a.X);
        }

        [TestMethod]
        public void Vector3SubtractTest()
        {
            Vector3 a = new(1, 1, 1);
            Vector3 b = new(1, 1, 1);

            Assert.AreEqual(new(0, 0, 0), a - 1f);
            Assert.AreEqual(new(0, 0, 0), a - b);

            Assert.AreEqual(new(1, 1, 1), a);
            Assert.AreEqual(new(1, 1, 1), b);

            a.Subtract(b);

            Assert.AreEqual(new(0, 0, 0), a);
            Assert.AreEqual(new(1, 1, 1), b);

            a.Subtract(1f);

            Assert.AreEqual(new(-1, -1, -1), a);
        }

        [TestMethod]
        public void Vector3ZeroTest()
        {
            Vector3 a = new(0, 0, 0);
            Assert.AreEqual(a, Vector3.Zero);
        }
    }
}