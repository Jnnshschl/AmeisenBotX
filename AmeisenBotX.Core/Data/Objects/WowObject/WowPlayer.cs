using AmeisenBotX.Core.Data.Enums;
using System;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowPlayer : WowUnit
    {
        public int Exp { get; set; }

        public int MaxExp { get; set; }

        public int ComboPoints { get; set; }

        public bool IsAlliance()
            => Race == WowRace.Draenei || Race == WowRace.Human || Race == WowRace.Dwarf || Race == WowRace.Gnome || Race == WowRace.Nightelf;

        public bool IsHorde()
            => Race == WowRace.Undead || Race == WowRace.Orc || Race == WowRace.Bloodelf || Race == WowRace.Tauren || Race == WowRace.Undead;
    }
}
