using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Chat;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Db;
using AmeisenBotX.Core.Data.CombatLog;
using AmeisenBotX.Core.Dungeon;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Grinding;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Pathfinding;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.Offsets;
using AmeisenBotX.Core.Personality;
using AmeisenBotX.Core.Quest;
using AmeisenBotX.Core.Statemachine.CombatClasses;
using AmeisenBotX.Core.Tactic;
using AmeisenBotX.Memory;
using AmeisenBotX.RconClient;
using System.Diagnostics;

namespace AmeisenBotX.Core
{
    public class WowInterface
    {
        private static WowInterface i;

        private WowInterface()
        {
        }

        public static WowInterface I => i ??= new WowInterface();

        public IBattlegroundEngine BattlegroundEngine { get; set; }

        public IAmeisenBotDb Db { get; set; }

        public BotPersonality Personality { get; set; }

        public ICharacterManager CharacterManager { get; set; }

        public ChatManager ChatManager { get; set; }

        public ICombatClass CombatClass { get; set; }

        public CombatLogParser CombatLogParser { get; set; }

        public IDungeonEngine DungeonEngine { get; set; }

        public EventHook EventHookManager { get; set; }

        public AmeisenBotGlobals Globals { get; set; }

        public GrindingEngine GrindingEngine { get; set; }

        public IHookManager HookManager { get; set; }

        public JobEngine JobEngine { get; set; }

        public IMovementEngine MovementEngine { get; set; }

        public MovementSettings MovementSettings { get; set; }

        public IObjectManager ObjectManager { get; set; }

        public IOffsetList OffsetList { get; set; }

        public IPathfindingHandler PathfindingHandler { get; set; }

        public QuestEngine QuestEngine { get; set; }

        public AmeisenBotRconClient RconClient { get; set; }

        public TacticEngine TacticEngine { get; set; }

        public Process WowProcess { get; set; }

        public XMemory XMemory { get; set; }
    }
}