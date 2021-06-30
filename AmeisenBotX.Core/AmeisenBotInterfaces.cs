using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Battleground;
using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Chat;
using AmeisenBotX.Core.Combat.Classes;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Dungeon;
using AmeisenBotX.Core.Event;
using AmeisenBotX.Core.Grinding;
using AmeisenBotX.Core.Jobs;
using AmeisenBotX.Core.Movement;
using AmeisenBotX.Core.Movement.Pathfinding;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.Quest;
using AmeisenBotX.Core.Tactic;
using AmeisenBotX.Memory;
using AmeisenBotX.RconClient;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Cache;
using AmeisenBotX.Wow.Combatlog;
using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core
{
    public class AmeisenBotInterfaces
    {
        public IBattlegroundEngine Battleground { get; set; }

        public ICharacterManager Character { get; set; }

        public ChatManager Chat { get; set; }

        public ICombatClass CombatClass { get; set; }

        public CombatLogParser CombatLog { get; set; }

        public IAmeisenBotDb Db { get; set; }

        public IDungeonEngine Dungeon { get; set; }

        public EventHook Events { get; set; }

        public AmeisenBotGlobals Globals { get; set; }

        public GrindingEngine Grinding { get; set; }

        public JobEngine Jobs { get; set; }

        public WowUnit LastTarget => Objects.LastTarget;

        public XMemory Memory { get; set; }

        public IMovementEngine Movement { get; set; }

        public MovementSettings MovementSettings { get; set; }

        public IObjectProvider Objects => Wow.Objects;

        public IOffsetList Offsets { get; set; }

        public IPathfindingHandler PathfindingHandler { get; set; }

        public WowUnit Pet => Objects.Pet;

        public WowPlayer Player => Objects.Player;

        public QuestEngine Quest { get; set; }

        public AmeisenBotRconClient Rcon { get; set; }

        public TacticEngine Tactic { get; set; }

        public WowUnit Target => Objects.Target;

        public IWowInterface Wow { get; set; }
    }
}