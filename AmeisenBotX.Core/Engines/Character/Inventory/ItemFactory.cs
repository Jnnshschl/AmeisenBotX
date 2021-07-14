using AmeisenBotX.Core.Engines.Character.Inventory.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;

namespace AmeisenBotX.Core.Engines.Character.Inventory
{
    public static class ItemFactory
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            Error = (sender, errorArgs) => errorArgs.ErrorContext.Handled = true
        };

        public static WowBasicItem BuildSpecificItem(WowBasicItem basicItem)
        {
            return basicItem?.Type.ToUpper(CultureInfo.InvariantCulture) switch
            {
                "ARMOR" => new WowArmor(basicItem),
                "CONSUMABLE" => new WowConsumable(basicItem),
                "CONTAINER" => new WowContainer(basicItem),
                "GEM" => new WowGem(basicItem),
                "KEY" => new WowKey(basicItem),
                "MISCELLANEOUS" => new WowMiscellaneousItem(basicItem),
                "MONEY" => new WowMoneyItem(basicItem),
                "PROJECTILE" => new WowProjectile(basicItem),
                "QUEST" => new WowQuestItem(basicItem),
                "QUIVER" => new WowQuiver(basicItem),
                "REAGENT" => new WowReagent(basicItem),
                "RECIPE" => new WowRecipe(basicItem),
                "TRADE GOODS" => new WowTradegood(basicItem),
                "WEAPON" => new WowWeapon(basicItem),
                _ => basicItem,
            };
        }

        public static WowBasicItem ParseItem(string json)
        {
            return JsonConvert.DeserializeObject<WowBasicItem>(json, JsonSerializerSettings);
        }

        public static List<WowBasicItem> ParseItemList(string json)
        {
            return JsonConvert.DeserializeObject<List<WowBasicItem>>(json, JsonSerializerSettings);
        }
    }
}