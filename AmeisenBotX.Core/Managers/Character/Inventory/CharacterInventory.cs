using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Managers.Character.Inventory
{
    public class CharacterInventory
    {
        private readonly List<IWowInventoryItem> items;

        private readonly object queryLock = new();

        public CharacterInventory(IWowInterface wowInterface, AmeisenBotConfig config)
        {
            Wow = wowInterface;
            Config = config;
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

        private AmeisenBotConfig Config { get; }

        private bool ConfirmDelete { get; set; }

        private DateTime ConfirmDeleteTime { get; set; }

        private IWowInterface Wow { get; }

        public void DestroyItemByName(string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (HasItemByName(name, stringComparison))
            {
                Wow.DeleteItemByName(name);
            }
        }

        public bool HasItemByName(string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return Items.Any(e => e.Name.Equals(name, stringComparison));
        }

        public void OnStaticPopupDeleteItem(int id)
        {
            ConfirmDelete = false;
            Wow.ClickUiElement($"StaticPopup{id}Button1");
            AmeisenLogger.I.Log("Inventory", $"Confirmed Deleting");
        }

        public void TryDestroyTrash(WowItemQuality maxQuality = WowItemQuality.Poor)
        {
            if (DateTime.Now - ConfirmDeleteTime > TimeSpan.FromSeconds(10))
            {
                // after 10s reset confirm stuff
                ConfirmDelete = false;
            }
            else if (ConfirmDelete)
            {
                // still waiting to confirm deletion
                return;
            }

            foreach (IWowInventoryItem item in Items.Where(e => e.Price > 0 && e.ItemQuality == (int)maxQuality).OrderBy(e => e.Price))
            {
                if (!Config.ItemSellBlacklist.Any(e => e.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    AmeisenLogger.I.Log("Inventory", $"Deleting Trash: {item.Name}");
                    Wow.DeleteItemByName(item.Name);
                    ConfirmDelete = true;
                    ConfirmDeleteTime = DateTime.Now;
                    break;
                }
            }
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