using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterInventory
    {
        public CharacterInventory(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Items = new List<IWowItem>();
        }

        public List<IWowItem> Items { get; private set; }

        private WowInterface WowInterface { get; }

        public void Update()
        {
            string resultJson = WowInterface.HookManager.GetInventoryItems();

            try
            {
                List<WowBasicItem> basicItems = ItemFactory.ParseItemList(resultJson);

                Items.Clear();
                foreach (WowBasicItem basicItem in basicItems)
                {
                    Items.Add(ItemFactory.BuildSpecificItem(basicItem));
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("CharacterManager", $"Failed to parse Inventory JSON:\n{resultJson}\n{e.ToString()}", LogLevel.Error);
            }
        }
    }
}