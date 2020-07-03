using AmeisenBotX.Core.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class TimegatesEventTests
    {
        [TestMethod]
        public void TimegateGenericTest()
        {
            int counter = 0;
            TimegatedEvent<bool> eventA = new TimegatedEvent<bool>(TimeSpan.FromMilliseconds(1), () => { return true; });

            for (int i = 0; i < 6; ++i)
            {
                if (eventA.Run(out bool _))
                {
                    ++counter;
                }

                Thread.Sleep(1);
            }

            Assert.AreEqual(6, counter);
        }

        [TestMethod]
        public void TimegateTest()
        {
            int counter = 0;
            TimegatedEvent eventA = new TimegatedEvent(TimeSpan.FromMilliseconds(1), () => { ++counter; });

            for (int i = 0; i < 6; ++i)
            {
                eventA.Run();
                Thread.Sleep(1);
            }

            Assert.AreEqual(6, counter);
        }
    }
}