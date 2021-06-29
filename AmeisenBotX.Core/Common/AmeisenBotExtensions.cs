using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Common
{
    /// <summary>
    /// Collection of extensions to mainly enums at the moment.
    /// </summary>
    public static class AmeisenBotExtensions
    {
        public static string GetColor(this WowItemQuality itemQuality)
        {
            return itemQuality switch
            {
                WowItemQuality.Unique => "#00ccff",
                WowItemQuality.Poor => "#9d9d9d",
                WowItemQuality.Common => "#ffffff",
                WowItemQuality.Uncommon => "#1eff00",
                WowItemQuality.Rare => "#0070dd",
                WowItemQuality.Epic => "#a335ee",
                WowItemQuality.Legendary => "#ff8000",
                WowItemQuality.Artifact => "#e6cc80",
                _ => "#ffffff",
            };
        }

        public static bool IsBattlegroundMap(this WowMapId map)
        {
            return map == WowMapId.AlteracValley
                || map == WowMapId.WarsongGulch
                || map == WowMapId.ArathiBasin
                || map == WowMapId.EyeOfTheStorm
                || map == WowMapId.StrandOfTheAncients;
        }

        public static bool IsCapitalCityZone(this WowZoneId zone, bool isAlliance)
        {
            if (isAlliance)
            {
                return zone == WowZoneId.StormwindCity
                    || zone == WowZoneId.Ironforge
                    || zone == WowZoneId.Teldrassil
                    || zone == WowZoneId.TheExodar;
            }
            else
            {
                return zone == WowZoneId.Orgrimmar
                    || zone == WowZoneId.Undercity
                    || zone == WowZoneId.ThunderBluff
                    || zone == WowZoneId.SilvermoonCity;
            }
        }

        public static bool IsDungeonMap(this WowMapId map)
        {
            // classic dungeon
            return map == WowMapId.RagefireChasm
                || map == WowMapId.WailingCaverns
                || map == WowMapId.Deadmines
                || map == WowMapId.ShadowfangKeep
                || map == WowMapId.StormwindStockade
                // tbc dungeons
                || map == WowMapId.HellfireRamparts
                || map == WowMapId.TheBloodFurnace
                || map == WowMapId.TheSlavePens
                || map == WowMapId.TheUnderbog
                || map == WowMapId.TheSteamvault
                // wotlk dungeons
                || map == WowMapId.UtgardeKeep
                || map == WowMapId.AzjolNerub
                || map == WowMapId.TheForgeOfSouls
                || map == WowMapId.PitOfSaron;
        }
    }
}