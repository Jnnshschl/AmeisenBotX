using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.Routines
{
    public static class SellItemsRoutine
    {
        public static void Run(WowInterface wowInterface, AmeisenBotConfig config)
        {
            foreach (IWowItem item in wowInterface.CharacterManager.Inventory.Items.Where(e => e.Price > 0))
            {
                IWowItem itemToSell = item;

                if (config.ItemSellBlacklist.Any(e => e.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                    || (!config.SellGrayItems && item.ItemQuality == ItemQuality.Poor)
                    || (!config.SellWhiteItems && item.ItemQuality == ItemQuality.Common)
                    || (!config.SellGreenItems && item.ItemQuality == ItemQuality.Uncommon)
                    || (!config.SellBlueItems && item.ItemQuality == ItemQuality.Rare)
                    || (!config.SellPurpleItems && item.ItemQuality == ItemQuality.Epic))
                {
                    continue;
                }

                if (wowInterface.CharacterManager.IsItemAnImprovement(item, out IWowItem itemToReplace))
                {
                    // equip item and sell the other after
                    itemToSell = itemToReplace;
                    wowInterface.HookManager.LuaEquipItem(item, itemToReplace);
                }

                if (itemToSell != null
                    && (wowInterface.ObjectManager.Player.Class != WowClass.Hunter || itemToSell.GetType() != typeof(WowProjectile)))
                {
                    wowInterface.HookManager.LuaUseContainerItem(itemToSell.BagId, itemToSell.BagSlot);
                    wowInterface.HookManager.LuaCofirmStaticPopup();
                }
            }
        }
    }
}