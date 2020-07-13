using AmeisenBotX.Core.Data.Enums;

namespace AmeisenBotX.Core.Common
{
    public static class AmeisenBotExtensions
    {
        internal static bool IsBattlegroundMap(this MapId map)
        {
            return map == MapId.AlteracValley
                       || map == MapId.WarsongGulch
                       || map == MapId.ArathiBasin
                       || map == MapId.EyeOfTheStorm
                       || map == MapId.StrandOfTheAncients;
        }

        internal static bool IsCapitalCityZone(this ZoneId zone, bool isAlliance)
        {
            if (isAlliance)
            {
                return zone == ZoneId.StormwindCity
                            || zone == ZoneId.Ironforge
                            || zone == ZoneId.Teldrassil
                            || zone == ZoneId.TheExodar;
            }
            else
            {
                return zone == ZoneId.Orgrimmar
                            || zone == ZoneId.Undercity
                            || zone == ZoneId.ThunderBluff
                            || zone == ZoneId.SilvermoonCity;
            }
        }

        internal static bool IsDungeonMap(this MapId map)
        {
            return map == MapId.RagefireChasm
                       || map == MapId.WailingCaverns
                       || map == MapId.Deadmines
                       || map == MapId.ShadowfangKeep
                       || map == MapId.StormwindStockade

                       || map == MapId.HellfireRamparts
                       || map == MapId.TheBloodFurnace
                       || map == MapId.TheSlavePens
                       || map == MapId.TheUnderbog
                       || map == MapId.TheSteamvault

                       || map == MapId.UtgardeKeep
                       || map == MapId.AzjolNerub;
        }
    }
}