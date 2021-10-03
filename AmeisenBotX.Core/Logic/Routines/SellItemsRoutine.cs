using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Routines
{
    public static class SellItemsRoutine
    {
        public static void Run(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            // create a copy of items here to prevent updates while selling
            foreach (IWowInventoryItem item in bot.Character.Inventory.Items
                .Where(e => e is { Price: > 0 })
                .ToList())
            {
                IWowInventoryItem itemToSell = item;

                if (config.ItemSellBlacklist.Any(e => e.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                    || !config.SellGrayItems && item.ItemQuality == (int)WowItemQuality.Poor
                    || !config.SellWhiteItems && item.ItemQuality == (int)WowItemQuality.Common
                    || !config.SellGreenItems && item.ItemQuality == (int)WowItemQuality.Uncommon
                    || !config.SellBlueItems && item.ItemQuality == (int)WowItemQuality.Rare
                    || !config.SellPurpleItems && item.ItemQuality == (int)WowItemQuality.Epic)
                {
                    continue;
                }

                if (bot.Character.IsItemAnImprovement(itemToSell, out IWowInventoryItem itemToReplace)
                    && itemToReplace != null)
                {
                    // equip item and sell the other after
                    itemToSell = itemToReplace;
                    bot.Wow.EquipItem(item.Name, itemToReplace.EquipSlot);
                }

                if (bot.Objects.Player.Class == WowClass.Hunter &&
                    itemToSell.GetType() == typeof(WowProjectile)) continue;

                bot.Wow.UseContainerItem(itemToSell.BagId, itemToSell.BagSlot);
                bot.Wow.CofirmStaticPopup();
            }
            bot.Wow.ClearTarget();
        }
    }
}