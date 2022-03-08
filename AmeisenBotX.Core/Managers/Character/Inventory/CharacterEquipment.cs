using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Managers.Character.Inventory
{
    public class CharacterEquipment
    {
        private readonly object queryLock = new();
        private readonly Dictionary<WowEquipmentSlot, IWowInventoryItem> items;

        public CharacterEquipment(IWowInterface wowInterface)
        {
            Wow = wowInterface;
            Items = new Dictionary<WowEquipmentSlot, IWowInventoryItem>();
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
            private init
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
            if (!Items.ContainsKey(slot) || Items[slot].Id <= 0)
            {
                return false;
            }

            IWowItem item = Wow.ObjectProvider.WowObjects.OfType<IWowItem>()
                .FirstOrDefault(e => e.EntryId == Items[slot].Id);

            return item != null && item.ItemEnchantments.Any(e =>
                e.Id == enchantmentId);
        }

        public void Update()
        {
            string resultJson = Wow.GetEquipmentItems();

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

                        foreach (WowBasicItem item in rawEquipment.Select(ItemFactory.BuildSpecificItem))
                        {
                            Items.Add((WowEquipmentSlot)((IWowInventoryItem)item).EquipSlot, item);
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

            foreach (object enumValue in enumValues)
            {
                WowEquipmentSlot slot = (WowEquipmentSlot)enumValue;

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