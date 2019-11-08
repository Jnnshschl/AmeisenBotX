using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterInventory
    {
        public CharacterInventory(HookManager hookManager)
        {
            HookManager = hookManager;
            Items = new List<IWowItem>();
        }

        public List<IWowItem> Items { get; private set; }

        private HookManager HookManager { get; }

        public void Update()
        {
            string resultJson = HookManager.GetInventoryItems();

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
                AmeisenLogger.Instance.Log($"Failed to parse Inventory JSON:\n{resultJson}\n{e.ToString()}", LogLevel.Error);
            }
        }
    }
}
