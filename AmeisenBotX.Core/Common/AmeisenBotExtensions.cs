using AmeisenBotX.Core.Data.Enums;

namespace AmeisenBotX.Core.Common
{
    public static class AmeisenBotExtensions
    {
        internal static bool IsBattlegroundMap(this WowMapId map)
        {
            return map == WowMapId.AlteracValley
                || map == WowMapId.WarsongGulch
                || map == WowMapId.ArathiBasin
                || map == WowMapId.EyeOfTheStorm
                || map == WowMapId.StrandOfTheAncients;
        }

        internal static bool IsCapitalCityZone(this WowZoneId zone, bool isAlliance)
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

        internal static bool IsDungeonMap(this WowMapId map)
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