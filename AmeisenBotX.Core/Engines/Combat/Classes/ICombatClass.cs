using AmeisenBotX.Common.Storage;
using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Classes
{
    public interface ICombatClass : IStoreable
    {
        /// <summary>
        /// Who built the combat class?
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Targets that should not be attacked, this will be set by the dungeon engine for exmaple.
        /// </summary>
        IEnumerable<int> BlacklistedTargetDisplayIds { get; set; }

        /// <summary>
        /// Dynamic config values specific for your combat class.
        /// </summary>
        Dictionary<string, dynamic> Configurables { get; set; }

        /// <summary>
        /// Short description how this class is going to play or what it needs.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Name that will be displayed in the bot.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Set to true when you want to handle the facing stuff yourself.
        /// </summary>
        bool HandlesFacing { get; }

        /// <summary>
        /// Set to true when you want to handle movement yourself.
        /// </summary>
        bool HandlesMovement { get; }

        /// <summary>
        /// Set to true if you want to perform close combat.
        /// </summary>
        bool IsMelee { get; }

        /// <summary>
        /// This is the item comparator that the class will use to determine whether an item is better or not.
        /// </summary>
        IItemComparator ItemComparator { get; }

        /// <summary>
        /// This will contain units that need to be focused, for example keleseths iceblocks in utgarde. Will be set by the dungeon engine.
        /// </summary>
        IEnumerable<int> PriorityTargetDisplayIds { get; set; }

        /// <summary>
        /// DPS, Heal or Tank.
        /// </summary>
        WowRole Role { get; }

        /// <summary>
        /// Set the talents the bot will skill in here.
        /// </summary>
        TalentTree Talents { get; }

        /// <summary>
        /// Version of the combat class.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Only used when you dont handle movement yourself. If this is true, the bot will always try to stay behind the target.
        /// </summary>
        bool WalkBehindEnemy { get; }

        /// <summary>
        /// For which wow class this combat class is useable.
        /// </summary>
        WowClass WowClass { get; }

        /// <summary>
        /// Start Attacking the target;
        /// </summary>
        void AttackTarget();

        /// <summary>
        /// Will be polled in combat.
        /// </summary>
        void Execute();

        /// <summary>
        /// Will be polled out of combat.
        /// </summary>
        void OutOfCombatExecute();
    }
}