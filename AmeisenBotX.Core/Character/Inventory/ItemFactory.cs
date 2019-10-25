using AmeisenBotX.Core.Character.Inventory.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory
{
    public static class ItemFactory
    {
        public static WowBasicItem ParseItem(string json)
        {
            return JsonConvert.DeserializeObject<WowBasicItem>(json);
        }

        public static List<WowBasicItem> ParseItemList(string json)
        {
            return JsonConvert.DeserializeObject<List<WowBasicItem>>(json);
        }

        public static WowBasicItem BuildSpecificItem(WowBasicItem basicItem)
        {
            switch (basicItem.Type.ToUpper())
            {
                case "ARMOR": return new WowArmor(basicItem);
                case "CONSUMEABLE": return new WowConsumable(basicItem);
                case "CONTAINER": return new WowContainer(basicItem);
                case "GEM": return new WowGem(basicItem);
                case "KEY": return new WowKey(basicItem);
                case "MISCELLANEOUS": return new WowMiscellaneousItem(basicItem);
                case "MONEY": return new WowMoneyItem(basicItem);
                case "PROJECTILE": return new WowProjectile(basicItem);
                case "QUEST": return new WowQuestItem(basicItem);
                case "QUIVER": return new WowQuiver(basicItem);
                case "REAGENT": return new WowReagent(basicItem);
                case "RECIPE": return new WowRecipe(basicItem);
                case "TRADE GOODS": return new WowTradegood(basicItem);
                case "WEAPON": return new WowWeapon(basicItem);
                default: return basicItem;
            }
        }
    }
}
