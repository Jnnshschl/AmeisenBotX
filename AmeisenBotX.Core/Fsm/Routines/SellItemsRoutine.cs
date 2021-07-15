using AmeisenBotX.Core.Engines.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.Routines
{
    public static class SellItemsRoutine
    {
        public static void Run(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            // create a copy here to prevent updates while selling
            foreach (IWowInventoryItem item in bot.Character.Inventory.Items.Where(e => e.Price > 0).ToList())
            {
                IWowInventoryItem itemToSell = item;

                if (config.ItemSellBlacklist.Any(e => e.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                    || (!config.SellGrayItems && item.ItemQuality == (int)WowItemQuality.Poor)
                    || (!config.SellWhiteItems && item.ItemQuality == (int)WowItemQuality.Common)
                    || (!config.SellGreenItems && item.ItemQuality == (int)WowItemQuality.Uncommon)
                    || (!config.SellBlueItems && item.ItemQuality == (int)WowItemQuality.Rare)
                    || (!config.SellPurpleItems && item.ItemQuality == (int)WowItemQuality.Epic))
                {
                    continue;
                }

                if (bot.Character.IsItemAnImprovement(item, out IWowInventoryItem itemToReplace))
                {
                    // equip item and sell the other after
                    itemToSell = itemToReplace;
                    bot.Wow.LuaEquipItem(item.Name/*, itemToReplace*/);
                }

                if (itemToSell != null
                    && (bot.Objects.Player.Class != WowClass.Hunter || itemToSell.GetType() != typeof(WowProjectile)))
                {
                    bot.Wow.LuaUseContainerItem(itemToSell.BagId, itemToSell.BagSlot);
                    bot.Wow.LuaCofirmStaticPopup();
                }
            }
        }
    }
}