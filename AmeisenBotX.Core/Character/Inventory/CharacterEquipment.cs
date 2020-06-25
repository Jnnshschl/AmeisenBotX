using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs.SubStructs;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterEquipment
    {
        private readonly object queryLock = new object();
        private Dictionary<EquipmentSlot, IWowItem> items;

        public CharacterEquipment(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            Items = new Dictionary<EquipmentSlot, IWowItem>();
        }

        public double AverageItemLevel { get; private set; }

        public Dictionary<EquipmentSlot, IWowItem> Items
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

        private WowInterface WowInterface { get; }

        public bool HasEnchantment(EquipmentSlot slot, int enchantmentId)
        {
            if (WowInterface.CharacterManager.Equipment.Items.ContainsKey(slot))
            {
                int itemId = Items[slot].Id;

                if (itemId > 0)
                {
                    WowItem item = WowInterface.ObjectManager.WowObjects.OfType<WowItem>().FirstOrDefault(e => e.EntryId == itemId);

                    List<ItemEnchantment> enchantments = item.GetItemEnchantments();

                    if (item != null && enchantments.Any(e => e.Id == enchantmentId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Update()
        {
            string resultJson = WowInterface.HookManager.GetEquipmentItems();
            if (resultJson.Length > 1 && resultJson.Substring(resultJson.Length - 2, 2).Equals(",]"))
            {
                resultJson.Remove(resultJson.Length - 2);
            }

            try
            {
                List<WowBasicItem> rawEquipment = ItemFactory.ParseItemList(resultJson);

                if (rawEquipment != null && rawEquipment.Count > 0)
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
                AmeisenLogger.Instance.Log("CharacterManager", $"Failed to parse Equipment JSON:\n{resultJson}\n{e}", LogLevel.Error);
            }
        }

        private double GetAverageItemLevel()
        {
            double itemLevel = 0.0;
            int count = 0;

            System.Collections.IList enumValues = Enum.GetValues(typeof(EquipmentSlot));

            for (int i = 0; i < enumValues.Count; ++i)
            {
                EquipmentSlot slot = (EquipmentSlot)enumValues[i];
                if (slot == EquipmentSlot.CONTAINER_BAG_1
                    || slot == EquipmentSlot.CONTAINER_BAG_2
                    || slot == EquipmentSlot.CONTAINER_BAG_3
                    || slot == EquipmentSlot.CONTAINER_BAG_4
                    || slot == EquipmentSlot.INVSLOT_OFFHAND
                    || slot == EquipmentSlot.INVSLOT_TABARD
                    || slot == EquipmentSlot.INVSLOT_AMMO
                    || slot == EquipmentSlot.NOT_EQUIPABLE)
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