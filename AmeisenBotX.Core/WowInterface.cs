using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.CombatLog;
using AmeisenBotX.Core.Data.Persistence;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs;
using AmeisenBotX.Core.LoginHandler;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Core.Personality;
using AmeisenBotX.Core.StateMachine.CombatClasses;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding;
using System.Diagnostics;

namespace AmeisenBotX.Core
{
    public class WowInterface
    {
        public BattlegroundEngine BattlegroundEngine { get; set; }

        public IAmeisenBotCache BotCache { get; set; }

        public BotPersonality BotPersonality { get; set; }

        public ICharacterManager CharacterManager { get; set; }

        public ICombatClass CombatClass { get; set; }

        public CombatLogParser CombatLogParser { get; set; }

        public EventHookManager EventHookManager { get; set; }

        public IHookManager HookManager { get; set; }

        public JobEngine JobEngine { get; set; }

        public DefaultLoginHandler LoginHandler { get; internal set; }

        public IMovementEngine MovementEngine { get; set; }

        public MovementSettings MovementSettings { get; set; }

        public IObjectManager ObjectManager { get; set; }

        public IOffsetList OffsetList { get; set; }

        public IPathfindingHandler PathfindingHandler { get; set; }

        public Process WowProcess { get; set; }

        public XMemory XMemory { get; set; }
    }
}