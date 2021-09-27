using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;

namespace AmeisenBotX.Core.Managers.Character.Inventory
{
    public class CharacterInventory
    {
        private readonly object queryLock = new();
        private List<IWowInventoryItem> items;

        public CharacterInventory(IWowInterface wowInterface)
        {
            Wow = wowInterface;
            Items = new();
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
            set
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