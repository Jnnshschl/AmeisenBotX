using AmeisenBotX.Core.Character.Inventory;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AmeisenBotX.Test
{
    [TestClass]
    public class CharacterManagerTests
    {
        public string testItem =
            "{" +
                "\"id\": \"1337\"," +
                "\"count\": \"1\"," +
                "\"quality\": \"1\"," +
                "\"curDurability\": \"10\"," +
                "\"maxDurability\": \"20\"," +
                "\"cooldownStart\": \"5\"," +
                "\"cooldownEnd\": \"8\"," +
                "\"name\": \"TestItem\"," +
                "\"link\": \"[TestItem]\"," +
                "\"level\": \"69\"," +
                "\"minLevel\": \"64\"," +
                "\"type\": \"MISCELLANEOUS\"," +
                "\"subtype\": \"NONE\"," +
                "\"maxStack\": \"1\"," +
                "\"equiplocation\": \"NOT_EQUIPABLE\"," +
                "\"sellprice\": \"650\"" +
            "}";

        public string testItemList =
            "[{" +
                "\"id\": \"1337\"," +
                "\"count\": \"1\"," +
                "\"quality\": \"1\"," +
                "\"curDurability\": \"10\"," +
                "\"maxDurability\": \"20\"," +
                "\"cooldownStart\": \"5\"," +
                "\"cooldownEnd\": \"8\"," +
                "\"name\": \"TestItem\"," +
                "\"link\": \"[TestItem]\"," +
                "\"level\": \"69\"," +
                "\"minLevel\": \"64\"," +
                "\"type\": \"MISCELLANEOUS\"," +
                "\"subtype\": \"NONE\"," +
                "\"maxStack\": \"1\"," +
                "\"equiplocation\": \"NOT_EQUIPABLE\"," +
                "\"sellprice\": \"650\"" +
            "}," +
            "{" +
                "\"id\": \"1338\"," +
                "\"count\": \"1\"," +
                "\"quality\": \"1\"," +
                "\"curDurability\": \"10\"," +
                "\"maxDurability\": \"20\"," +
                "\"cooldownStart\": \"5\"," +
                "\"cooldownEnd\": \"8\"," +
                "\"name\": \"TestItem1\"," +
                "\"link\": \"[TestItem1]\"," +
                "\"level\": \"69\"," +
                "\"minLevel\": \"64\"," +
                "\"type\": \"ARMOR\"," +
                "\"subtype\": \"CLOTH\"," +
                "\"maxStack\": \"1\"," +
                "\"equiplocation\": \"INVSLOT_CHEST\"," +
                "\"sellprice\": \"6500\"" +
            "}," +
            "{" +
                "\"id\": \"1339\"," +
                "\"count\": \"1\"," +
                "\"quality\": \"1\"," +
                "\"curDurability\": \"10\"," +
                "\"maxDurability\": \"20\"," +
                "\"cooldownStart\": \"5\"," +
                "\"cooldownEnd\": \"8\"," +
                "\"name\": \"TestItem2\"," +
                "\"link\": \"[TestItem2]\"," +
                "\"level\": \"69\"," +
                "\"minLevel\": \"64\"," +
                "\"type\": \"WEAPON\"," +
                "\"subtype\": \"GUNS\"," +
                "\"maxStack\": \"1\"," +
                "\"equiplocation\": \"INVSLOT_RANGED\"," +
                "\"sellprice\": \"65\"" +
            "}]";

        [TestMethod]
        public void TestItemListParsing()
        {
            List<WowBasicItem> items = ItemFactory.ParseItemList(testItemList);
            Assert.IsInstanceOfType(items, typeof(List<WowBasicItem>));
            Assert.IsTrue(items.Count == 3);

            WowBasicItem item0 = ItemFactory.BuildSpecificItem(items[0]);
            WowBasicItem item1 = ItemFactory.BuildSpecificItem(items[1]);
            WowBasicItem item2 = ItemFactory.BuildSpecificItem(items[2]);

            Assert.IsInstanceOfType(item0, typeof(WowMiscellaneousItem));
            Assert.IsInstanceOfType(item1, typeof(WowArmor));
            Assert.IsInstanceOfType(item2, typeof(WowWeapon));

            Assert.AreEqual(EquipmentSlot.NOT_EQUIPABLE, item0.EquipSlot);

            Assert.AreEqual(ArmorType.CLOTH, ((WowArmor)item1).ArmorType);
            Assert.AreEqual(WeaponType.GUNS, ((WowWeapon)item2).WeaponType);
        }

        [TestMethod]
        public void TestItemParsing()
        {
            WowBasicItem item = ItemFactory.ParseItem(testItem);
            Assert.IsInstanceOfType(item, typeof(WowBasicItem));

            item = ItemFactory.BuildSpecificItem(item);
            Assert.IsInstanceOfType(item, typeof(WowMiscellaneousItem));

            Assert.AreEqual(1337, item.Id);
            Assert.AreEqual(1, item.Count);
            Assert.AreEqual(ItemQuality.Common, item.ItemQuality);
            Assert.AreEqual(10, item.Durability);
            Assert.AreEqual(20, item.MaxDurability);
            Assert.AreEqual("TestItem", item.Name);
            Assert.AreEqual("[TestItem]", item.ItemLink);
            Assert.AreEqual(69, item.ItemLevel);
            Assert.AreEqual(64, item.RequiredLevel);
            Assert.AreEqual("NONE", item.Subtype);
            Assert.AreEqual(1, item.MaxStack);
            Assert.AreEqual(EquipmentSlot.NOT_EQUIPABLE, item.EquipSlot);
            Assert.AreEqual(650, item.Price);
        }
    }
}