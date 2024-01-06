using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Core.Managers.Character.Inventory
{
    public static class ItemFactory
    {
        public static WowBasicItem BuildSpecificItem(WowBasicItem basicItem)
        {
            return basicItem == null
                ? throw new ArgumentNullException(nameof(basicItem), "basicItem cannot be null")
                : basicItem.Type == null
                ? basicItem
                : basicItem.Type.ToUpper(CultureInfo.InvariantCulture) switch
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
                    "TRADE GOODS" => new WowTradeGoods(basicItem),
                    "WEAPON" => new WowWeapon(basicItem),
                    _ => basicItem,
                };
        }

        public static WowBasicItem ParseItem(string json)
        {
            return JsonSerializer.Deserialize<WowBasicItem>(json, new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            });
        }

        public static List<WowBasicItem> ParseItemList(string json)
        {
            return JsonSerializer.Deserialize<List<WowBasicItem>>(json, new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            });
        }
    }
}