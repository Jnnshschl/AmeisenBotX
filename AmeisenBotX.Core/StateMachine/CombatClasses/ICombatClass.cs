using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Statemachine.CombatClasses
{
    public interface ICombatClass
    {
        string Author { get; }

        WowClass WowClass { get; }

        Dictionary<string, dynamic> Configureables { get; set; }

        string Description { get; }

        string Displayname { get; }

        bool HandlesMovement { get; }

        bool IsMelee { get; }

        IWowItemComparator ItemComparator { get; }

        IEnumerable<string> PriorityTargets { get; set; }

        CombatClassRole Role { get; }

        TalentTree Talents { get; }

        bool TargetInLineOfSight { get; set; }

        string Version { get; }

        bool WalkBehindEnemy { get; }

        void Execute();

        void OutOfCombatExecute();
    }
}