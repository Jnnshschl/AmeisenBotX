using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterEquipment
    {
        public CharacterEquipment(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            Items = new Dictionary<EquipmentSlot, IWowItem>();
        }

        public double AverageItemLevel { get; private set; }

        public Dictionary<EquipmentSlot, IWowItem> Items { get; private set; }

        private WowInterface WowInterface { get; }

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

                Items.Clear();
                foreach (WowBasicItem rawItem in rawEquipment)
                {
                    IWowItem item = ItemFactory.BuildSpecificItem(rawItem);
                    Items.Add(item.EquipSlot, item);
                }

                AverageItemLevel = GetAverageItemLevel();
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("CharacterManager", $"Failed to parse Equipment JSON:\n{resultJson}\n{e.ToString()}", LogLevel.Error);
            }
        }

        private double GetAverageItemLevel()
        {
            double itemLevel = 0.0;
            int count = 0;

            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
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

                count++;

                if (Items.ContainsKey(slot))
                {
                    itemLevel += Items[slot].ItemLevel;
                }

                itemLevel /= count;
            }

            return itemLevel;
        }
    }
}