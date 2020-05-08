using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class ObjectTests
    {
        [TestMethod]
        public void WowContainerSizeTest()
        {
            // >> WowObject : WowContainer
            int rawWowObjectSize = Marshal.SizeOf(typeof(RawWowObject));
            int rawWowContainerSize = Marshal.SizeOf(typeof(RawWowContainer));
            _ = new WowContainer(IntPtr.Zero, WowObjectType.Container);

            Assert.AreEqual(RawWowObject.EndOffset + RawWowContainer.EndOffset, rawWowObjectSize + rawWowContainerSize);
        }

        [TestMethod]
        public void WowCorpseSizeTest()
        {
            // >> WowObject : WowCorpse
            int rawWowObjectSize = Marshal.SizeOf(typeof(RawWowObject));
            int rawWowCorpseSize = Marshal.SizeOf(typeof(RawWowCorpse));
            _ = new WowCorpse(IntPtr.Zero, WowObjectType.Corpse);

            Assert.AreEqual(RawWowObject.EndOffset + RawWowCorpse.EndOffset, rawWowObjectSize + rawWowCorpseSize);
        }

        [TestMethod]
        public void WowDynobjectSizeTest()
        {
            // >> WowObject : WowDynobject
            int rawWowObjectSize = Marshal.SizeOf(typeof(RawWowObject));
            int rawWowDynobjectSize = Marshal.SizeOf(typeof(RawWowDynobject));
            _ = new WowDynobject(IntPtr.Zero, WowObjectType.Dynobject);

            Assert.AreEqual(RawWowObject.EndOffset + RawWowDynobject.EndOffset, rawWowObjectSize + rawWowDynobjectSize);
        }

        [TestMethod]
        public void WowGameobjectSizeTest()
        {
            // >> WowObject : WowGameobject
            int rawWowObjectSize = Marshal.SizeOf(typeof(RawWowObject));
            int rawWowGameobjectSize = Marshal.SizeOf(typeof(RawWowGameobject));
            _ = new WowGameobject(IntPtr.Zero, WowObjectType.Gameobject);

            Assert.AreEqual(RawWowObject.EndOffset + RawWowGameobject.EndOffset, rawWowObjectSize + rawWowGameobjectSize);
        }

        [TestMethod]
        public void WowItemSizeTest()
        {
            // >> WowObject : WowItem
            int rawWowObjectSize = Marshal.SizeOf(typeof(RawWowObject));
            int rawWowItemSize = Marshal.SizeOf(typeof(RawWowItem));
            _ = new WowItem(IntPtr.Zero, WowObjectType.Item);

            Assert.AreEqual(RawWowObject.EndOffset + RawWowItem.EndOffset, rawWowObjectSize + rawWowItemSize);
        }

        [TestMethod]
        public void WowObjectSizeTest()
        {
            // >> WowObject
            int rawWowObjectSize = Marshal.SizeOf(typeof(RawWowObject));
            _ = new WowObject(IntPtr.Zero, WowObjectType.None);

            Assert.AreEqual(RawWowObject.EndOffset, rawWowObjectSize);
        }

        [TestMethod]
        public void WowUnitSizeTest()
        {
            // >> WowObject : WowUnit
            int rawWowObjectSize = Marshal.SizeOf(typeof(RawWowObject));
            int rawWowUnitSize = Marshal.SizeOf(typeof(RawWowUnit));
            _ = new WowUnit(IntPtr.Zero, WowObjectType.Unit);

            Assert.AreEqual(RawWowObject.EndOffset + RawWowUnit.EndOffset, rawWowObjectSize + rawWowUnitSize);
        }

        // [TestMethod]
        // public void WowPlayerSizeTest()
        // {
        //     // >> WowObject : WowUnit : WowPlayer
        //     int rawWowObjectSize = Marshal.SizeOf(typeof(RawWowObject));
        //     int rawWowUnitSize = Marshal.SizeOf(typeof(RawWowUnit));
        //     int rawWowPlayerSize = Marshal.SizeOf(typeof(RawWowPlayer));
        //
        //     WowPlayer wowPlayer = new WowPlayer();
        //
        //     Assert.AreEqual(RawWowObject.EndOffset + RawWowUnit.EndOffset + RawWowPlayer.EndOffset, rawWowObjectSize + rawWowUnitSize + rawWowPlayerSize);
        // }
    }
}