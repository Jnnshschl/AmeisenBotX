using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterInventory
    {
        private readonly object queryLock = new object();
        private List<IWowItem> items;

        public CharacterInventory(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Items = new List<IWowItem>();
        }

        public List<IWowItem> Items
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

        public void Update()
        {
            string resultJson = WowInterface.HookManager.GetInventoryItems();

            try
            {
                List<WowBasicItem> basicItems = ItemFactory.ParseItemList(resultJson);

                lock (queryLock)
                {
                    Items.Clear();
                    foreach (WowBasicItem basicItem in basicItems)
                    {
                        Items.Add(ItemFactory.BuildSpecificItem(basicItem));
                    }
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.Instance.Log("CharacterManager", $"Failed to parse Inventory JSON:\n{resultJson}\n{e}", LogLevel.Error);
            }
        }
    }
}