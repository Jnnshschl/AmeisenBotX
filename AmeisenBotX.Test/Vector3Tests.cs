using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class Vector3Tests
    {
        [TestMethod]
        public void Vector3AddTest()
        {
            Vector3 a = new Vector3(1, 1, 1);
            Vector3 b = new Vector3(1, 1, 1);

            Assert.AreEqual(new Vector3(2, 2, 2), a + b);

            Assert.AreEqual(new Vector3(1, 1, 1), a);
            Assert.AreEqual(new Vector3(1, 1, 1), b);

            a.Add(b);

            Assert.AreEqual(new Vector3(2, 2, 2), a);
            Assert.AreEqual(new Vector3(1, 1, 1), b);
        }

        [TestMethod]
        public void Vector3SubtractTest()
        {
            Vector3 a = new Vector3(1, 1, 1);
            Vector3 b = new Vector3(1, 1, 1);

            Assert.AreEqual(new Vector3(0, 0, 0), a - b);

            Assert.AreEqual(new Vector3(1, 1, 1), a);
            Assert.AreEqual(new Vector3(1, 1, 1), b);

            a.Subtract(b);

            Assert.AreEqual(new Vector3(0, 0, 0), a);
            Assert.AreEqual(new Vector3(1, 1, 1), b);
        }

        [TestMethod]
        public void Vector3DivideTest()
        {
            Vector3 a = new Vector3(2, 2, 2);
            Vector3 b = new Vector3(2, 2, 2);

            Assert.AreEqual(new Vector3(1, 1, 1), a / b);

            Assert.AreEqual(new Vector3(2, 2, 2), a);
            Assert.AreEqual(new Vector3(2, 2, 2), b);

            a.Divide(b);

            Assert.AreEqual(new Vector3(1, 1, 1), a);
            Assert.AreEqual(new Vector3(2, 2, 2), b);
        }

        [TestMethod]
        public void Vector3ultiplyTest()
        {
            Vector3 a = new Vector3(2, 2, 2);
            Vector3 b = new Vector3(2, 2, 2);

            Assert.AreEqual(new Vector3(4, 4, 4), a * b);

            Assert.AreEqual(new Vector3(2, 2, 2), a);
            Assert.AreEqual(new Vector3(2, 2, 2), b);

            a.Multiply(b);

            Assert.AreEqual(new Vector3(4, 4, 4), a);
            Assert.AreEqual(new Vector3(2, 2, 2), b);
        }
    }
}
