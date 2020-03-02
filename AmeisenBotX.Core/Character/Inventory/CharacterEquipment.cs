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
            Equipment = new Dictionary<EquipmentSlot, IWowItem>();
        }

        public Dictionary<EquipmentSlot, IWowItem> Equipment { get; private set; }

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

                Equipment.Clear();
                foreach (WowBasicItem rawItem in rawEquipment)
                {
                    IWowItem item = ItemFactory.BuildSpecificItem(rawItem);
                    Equipment.Add(item.EquipSlot, item);
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("CharacterManager", $"Failed to parse Equipment JSON:\n{resultJson}\n{e.ToString()}", LogLevel.Error);
            }
        }
    }
}