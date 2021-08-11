using AmeisenBotX.Wow.Objects.SubStructs;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Objects
{
    public interface IWowPlayer : IWowUnit
    {
        int ComboPoints { get; }

        bool IsFlying { get; }

        bool IsGhost { get; }

        bool IsOutdoors { get; }

        bool IsSwimming { get; }

        bool IsUnderwater { get; }

        IEnumerable<VisibleItemEnchantment> ItemEnchantments { get; }

        int NextLevelXp { get; }

        IEnumerable<QuestlogEntry> QuestlogEntries { get; }

        int Xp { get; }

        double XpPercentage { get; }

        bool IsAlliance();

        bool IsHorde();
    }
}