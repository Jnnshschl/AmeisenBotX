using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core
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
            return map is WowMapId.AlteracValley
                or WowMapId.WarsongGulch
                or WowMapId.ArathiBasin
                or WowMapId.EyeOfTheStorm
                or WowMapId.StrandOfTheAncients;
        }

        public static bool IsCapitalCityZone(this WowZoneId zone, bool isAlliance)
        {
            return isAlliance
                ? zone is WowZoneId.StormwindCity
                    or WowZoneId.Ironforge
                    or WowZoneId.Teldrassil
                    or WowZoneId.TheExodar
                : zone is WowZoneId.Orgrimmar
                    or WowZoneId.Undercity
                    or WowZoneId.ThunderBluff
                    or WowZoneId.SilvermoonCity;
        }

        public static bool IsDungeonMap(this WowMapId map)
        {
            // classic dungeon
            return map is WowMapId.RagefireChasm
                or WowMapId.WailingCaverns
                or WowMapId.Deadmines
                or WowMapId.ShadowfangKeep
                or WowMapId.StormwindStockade
                // tbc dungeons
                or WowMapId.HellfireRamparts
                or WowMapId.TheBloodFurnace
                or WowMapId.TheSlavePens
                or WowMapId.TheUnderbog
                or WowMapId.TheSteamvault
                // wotlk dungeons
                or WowMapId.UtgardeKeep
                or WowMapId.AzjolNerub
                or WowMapId.TheForgeOfSouls
                or WowMapId.PitOfSaron;
        }
    }
}