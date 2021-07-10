using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Wow.Cache.Structs
{
    internal interface ILikeUnit
    {
        public bool LikesAlliance { get; set; }

        public bool LikesHorde { get; set; }

        public bool LikesUnit(IWowUnit wowUnit)
        {
            return (LikesAlliance && (wowUnit.Race == WowRace.Human || wowUnit.Race == WowRace.Gnome ||
                                      wowUnit.Race == WowRace.Draenei || wowUnit.Race == WowRace.Dwarf ||
                                      wowUnit.Race == WowRace.Nightelf)) ||
                   (LikesHorde && (wowUnit.Race == WowRace.Orc || wowUnit.Race == WowRace.Troll ||
                                   wowUnit.Race == WowRace.Bloodelf || wowUnit.Race == WowRace.Undead ||
                                   wowUnit.Race == WowRace.Tauren));
        }
    }
}