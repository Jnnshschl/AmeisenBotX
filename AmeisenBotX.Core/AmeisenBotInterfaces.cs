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

        public IChatManager Chat { get; set; }

        public ICombatClass CombatClass { get; set; }

        public ICombatLogParser CombatLog { get; set; }

        public IAmeisenBotDb Db { get; set; }

        public IDungeonEngine Dungeon { get; set; }

        public IEventManager Events { get; set; }

        public AmeisenBotGlobals Globals { get; set; }

        public IGrindingEngine Grinding { get; set; }

        public IJobEngine Jobs { get; set; }

        public WowUnit LastTarget => Objects.LastTarget;

        public IMemoryApi Memory { get; set; }

        public IMovementEngine Movement { get; set; }

        public MovementSettings MovementSettings { get; set; }

        public IObjectProvider Objects => Wow.Objects;

        public IOffsetList Offsets { get; set; }

        public IPathfindingHandler PathfindingHandler { get; set; }

        public WowUnit Pet => Objects.Pet;

        public WowPlayer Player => Objects.Player;

        public IQuestEngine Quest { get; set; }

        public AmeisenBotRconClient Rcon { get; set; }

        public ITacticEngine Tactic { get; set; }

        public WowUnit Target => Objects.Target;

        public IWowInterface Wow { get; set; }
    }
}