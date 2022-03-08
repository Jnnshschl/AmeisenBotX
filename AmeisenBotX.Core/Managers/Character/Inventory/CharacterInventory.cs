using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Managers.Character.Inventory
{
    public class CharacterInventory
    {
        private readonly object queryLock = new();
        private readonly List<IWowInventoryItem> items;

        public CharacterInventory(IWowInterface wowInterface)
        {
            Wow = wowInterface;
            Items = new List<IWowInventoryItem>();
        }

        public int FreeBagSlots { get; private set; }

        public List<IWowInventoryItem> Items
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

        public void DestroyItemByName(string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (!HasItemByName(name, stringComparison))
            {
                return;
            }

            Wow.DeleteItemByName(name);
        }

        public bool HasItemByName(string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return Items.Any(e => e.Name.Equals(name, stringComparison));
        }

        public void Update()
        {
            FreeBagSlots = Wow.GetFreeBagSlotCount();
            string resultJson = Wow.GetInventoryItems();

            try
            {
                List<WowBasicItem> basicItems = ItemFactory.ParseItemList(resultJson);

                if (basicItems is not { Count: > 0 })
                {
                    return;
                }

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
                AmeisenLogger.I.Log("CharacterManager", $"Failed to parse Inventory JSON:\n{resultJson}\n{e}", LogLevel.Error);
            }
        }
    }
}