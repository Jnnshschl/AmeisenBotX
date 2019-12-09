using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using System.Collections.Generic;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public interface ICombatClass
    {
        string Displayname { get; }

        string Version { get; }

        string Author { get; }

        string Description { get; }

        CombatClassRole Role { get; }

        WowClass Class { get; }

        bool HandlesMovement { get; }

        bool HandlesTargetSelection { get; }

        bool IsMelee { get; }

        IWowItemComparator ItemComparator { get; }

        Dictionary<string, dynamic> Configureables { get; set; }

        void Execute();

        void OutOfCombatExecute();
    }
}
