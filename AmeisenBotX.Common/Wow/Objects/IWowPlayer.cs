﻿using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Raw.SubStructs;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Objects
{
    public interface IWowPlayer : IWowUnit
    {
        int ComboPoints { get; }

        bool IsFlying { get; }

        bool IsGhost { get; }

        bool IsOutdoors { get; set; }

        bool IsSwimming { get; }

        bool IsUnderwater { get; }

        IEnumerable<VisibleItemEnchantment> ItemEnchantments { get; }

        int NextLevelXp { get; }

        IEnumerable<QuestlogEntry> QuestlogEntries { get; }

        public new WowObjectType Type => WowObjectType.Player;

        int Xp { get; }

        double XpPercentage { get; }

        bool IsAlliance();

        bool IsHorde();
    }
}