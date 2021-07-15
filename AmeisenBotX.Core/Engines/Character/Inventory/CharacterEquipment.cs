using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Character.Inventory
{
    public class CharacterEquipment
    {
        private readonly object queryLock = new();
        private Dictionary<WowEquipmentSlot, IWowInventoryItem> items;

        public CharacterEquipment(IWowInterface wowInterface)
        {
            Wow = wowInterface;

            Items = new();
        }

        public float AverageItemLevel { get; private set; }

        public Dictionary<WowEquipmentSlot, IWowInventoryItem> Items
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

        private IWowInterface Wow { get; }

        public bool HasEnchantment(WowEquipmentSlot slot, int enchantmentId)
        {
            if (Items.ContainsKey(slot) && Items[slot].Id > 0)
            {
                IWowItem item = Wow.ObjectProvider.WowObjects.OfType<IWowItem>().FirstOrDefault(e => e.EntryId == Items[slot].Id);

                if (item != null && item.ItemEnchantments.Any(e => e.Id == enchantmentId))
                {
                    return true;
                }
            }

            return false;
        }

        public void Update()
        {
            string resultJson = Wow.LuaGetEquipmentItems();

            if (string.IsNullOrWhiteSpace(resultJson))
            {
                return;
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
                            IWowInventoryItem item = ItemFactory.BuildSpecificItem(rawEquipment[i]);
                            Items.Add((WowEquipmentSlot)item.EquipSlot, item);
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

        private float GetAverageItemLevel()
        {
            float itemLevel = 0.0f;
            int count = 0;

            IList enumValues = Enum.GetValues(typeof(WowEquipmentSlot));

            for (int i = 0; i < enumValues.Count; ++i)
            {
                WowEquipmentSlot slot = (WowEquipmentSlot)enumValues[i];

                if (slot is WowEquipmentSlot.CONTAINER_BAG_1
                    or WowEquipmentSlot.CONTAINER_BAG_2
                    or WowEquipmentSlot.CONTAINER_BAG_3
                    or WowEquipmentSlot.CONTAINER_BAG_4
                    or WowEquipmentSlot.INVSLOT_OFFHAND
                    or WowEquipmentSlot.INVSLOT_TABARD
                    or WowEquipmentSlot.INVSLOT_AMMO
                    or WowEquipmentSlot.NOT_EQUIPABLE)
                {
                    continue;
                }

                if (Items.ContainsKey(slot))
                {
                    itemLevel += Items[slot].ItemLevel;
                }

                ++count;
            }

            return itemLevel /= count;
        }
    }
}