using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterEquipment
    {
        private readonly object queryLock = new();
        private Dictionary<WowEquipmentSlot, IWowItem> items;

        public CharacterEquipment(AmeisenBotInterfaces bot)
        {
            Bot = bot;

            Items = new();
        }

        public double AverageItemLevel { get; private set; }

        public Dictionary<WowEquipmentSlot, IWowItem> Items
        {
            get
            {
                lock (queryLock)
                {
                    return items;
                }
            }

            set
            {
                lock (queryLock)
                {
                    items = value;
                }
            }
        }

        private AmeisenBotInterfaces Bot { get; }

        public bool HasEnchantment(WowEquipmentSlot slot, int enchantmentId)
        {
            if (Bot.Character.Equipment.Items.ContainsKey(slot))
            {
                int itemId = Items[slot].Id;

                if (itemId > 0)
                {
                    WowItem item = Bot.Objects.WowObjects.OfType<WowItem>().FirstOrDefault(e => e.EntryId == itemId);

                    if (item != null && item.ItemEnchantments.Any(e => e.Id == enchantmentId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Update()
        {
            string resultJson = Bot.Wow.LuaGetEquipmentItems();

            if (string.IsNullOrWhiteSpace(resultJson))
            {
                return;
            }

            if (resultJson.Length > 1 && resultJson.Substring(resultJson.Length - 2, 2).Equals(",]", StringComparison.OrdinalIgnoreCase))
            {
                resultJson = resultJson.Remove(resultJson.Length - 2);
            }

            try
            {
                List<WowBasicItem> rawEquipment = ItemFactory.ParseItemList(resultJson);

                if (rawEquipment != null && rawEquipment.Any())
                {
                    lock (queryLock)
                    {
                        Items.Clear();

                        for (int i = 0; i < rawEquipment.Count; ++i)
                        {
                            IWowItem item = ItemFactory.BuildSpecificItem(rawEquipment[i]);
                            Items.Add(item.EquipSlot, item);
                        }
                    }
                }

                AverageItemLevel = GetAverageItemLevel();
            }
            catch (Exception e)
            {
                AmeisenLogger.I.Log("CharacterManager", $"Failed to parse Equipment JSON:\n{resultJson}\n{e}", LogLevel.Error);
            }
        }

        private double GetAverageItemLevel()
        {
            double itemLevel = 0.0;
            int count = 0;

            System.Collections.IList enumValues = Enum.GetValues(typeof(WowEquipmentSlot));

            for (int i = 0; i < enumValues.Count; ++i)
            {
                WowEquipmentSlot slot = (WowEquipmentSlot)enumValues[i];
                if (slot == WowEquipmentSlot.CONTAINER_BAG_1
                    || slot == WowEquipmentSlot.CONTAINER_BAG_2
                    || slot == WowEquipmentSlot.CONTAINER_BAG_3
                    || slot == WowEquipmentSlot.CONTAINER_BAG_4
                    || slot == WowEquipmentSlot.INVSLOT_OFFHAND
                    || slot == WowEquipmentSlot.INVSLOT_TABARD
                    || slot == WowEquipmentSlot.INVSLOT_AMMO
                    || slot == WowEquipmentSlot.NOT_EQUIPABLE)
                {
                    continue;
                }

                if (Items.ContainsKey(slot)) { itemLevel += Items[slot].ItemLevel; }
                ++count;
            }

            return itemLevel /= count;
        }
    }
}