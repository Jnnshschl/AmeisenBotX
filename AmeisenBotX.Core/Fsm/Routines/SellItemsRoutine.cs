using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.Routines
{
    public static class SellItemsRoutine
    {
        public static void Run(WowInterface wowInterface, AmeisenBotConfig config)
        {
            // create a copy here to prevent updates while selling
            foreach (IWowItem item in wowInterface.CharacterManager.Inventory.Items.Where(e => e.Price > 0).ToList())
            {
                IWowItem itemToSell = item;

                if (config.ItemSellBlacklist.Any(e => e.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                    || (!config.SellGrayItems && item.ItemQuality == WowItemQuality.Poor)
                    || (!config.SellWhiteItems && item.ItemQuality == WowItemQuality.Common)
                    || (!config.SellGreenItems && item.ItemQuality == WowItemQuality.Uncommon)
                    || (!config.SellBlueItems && item.ItemQuality == WowItemQuality.Rare)
                    || (!config.SellPurpleItems && item.ItemQuality == WowItemQuality.Epic))
                {
                    continue;
                }

                if (wowInterface.CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                {
                    // equip item and sell the other after
                    itemToSell = itemToReplace;
                    wowInterface.NewWowInterface.LuaEquipItem(item.Name/*, itemToReplace*/);
                }

                if (itemToSell != null
                    && (wowInterface.Objects.Player.Class != WowClass.Hunter || itemToSell.GetType() != typeof(WowProjectile)))
                {
                    wowInterface.NewWowInterface.LuaUseContainerItem(itemToSell.BagId, itemToSell.BagSlot);
                    wowInterface.NewWowInterface.LuaCofirmStaticPopup();
                }
            }
        }
    }
}