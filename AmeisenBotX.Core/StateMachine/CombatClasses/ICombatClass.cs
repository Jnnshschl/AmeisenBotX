using System;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using AmeisenBotX.Core.Data.Objects.WowObjects;

namespace AmeisenBotX.Core.Statemachine.CombatClasses
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

        IWowItemComparator ItemComparator { get; }

        IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        CombatClassRole Role { get; }

        TalentTree Talents { get; }

        string Version { get; }

        bool WalkBehindEnemy { get; }

        WowClass WowClass { get; }

        void Execute();

        void OutOfCombatExecute();

        void AttackTarget();

        bool IsTargetAttackable(WowUnit target);
    }
}