using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Combat.Classes
{
    public interface ICombatClass
    {
        string Author { get; }

        IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        Dictionary<string, dynamic> Configureables { get; set; }

        string Description { get; }

        string Displayname { get; }

        bool HandlesMovement { get; }

        bool IsMelee { get; }

        IItemComparator ItemComparator { get; }

        IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        WowRole Role { get; }

        TalentTree Talents { get; }

        string Version { get; }

        bool WalkBehindEnemy { get; }

        WowClass WowClass { get; }

        void AttackTarget();

        void Execute();

        bool IsTargetAttackable(WowUnit target);

        void OutOfCombatExecute();
    }
}