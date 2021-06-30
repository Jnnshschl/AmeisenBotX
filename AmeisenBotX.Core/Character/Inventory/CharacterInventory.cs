using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Character.Inventory
{
    public class CharacterInventory
    {
        private readonly object queryLock = new();
        private List<IWowItem> items;

        public CharacterInventory(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Items = new();
        }

        public int FreeBagSlots { get; private set; }

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

        private AmeisenBotInterfaces Bot { get; }

        public void DestroyItemByName(string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (!HasItemByName(name, stringComparison))
            {
                return;
            }

            Bot.Wow.LuaDeleteInventoryItemByName(name);
        }

        public bool HasItemByName(string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return Items.Any(e => e.Name.Equals(name, stringComparison));
        }

        public void Update()
        {
            FreeBagSlots = Bot.Wow.LuaGetFreeBagSlotCount();
            string resultJson = Bot.Wow.LuaGetInventoryItems();

            try
            {
                List<WowBasicItem> basicItems = ItemFactory.ParseItemList(resultJson);

                if (basicItems != null && basicItems.Count > 0)
                {
                    lock (queryLock)
                    {
                        Items.Clear();

                        for (int i = 0; i < basicItems.Count; ++i)
                        {
                            Items.Add(ItemFactory.BuildSpecificItem(basicItems[i]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                AmeisenLogger.I.Log("CharacterManager", $"Failed to parse Inventory JSON:\n{resultJson}\n{e}", LogLevel.Error);
            }
        }
    }
}