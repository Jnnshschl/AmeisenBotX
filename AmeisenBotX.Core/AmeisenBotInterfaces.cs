using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Battleground;
using AmeisenBotX.Core.Engines.Character;
using AmeisenBotX.Core.Engines.Chat;
using AmeisenBotX.Core.Engines.Combat.Classes;
using AmeisenBotX.Core.Engines.Dungeon;
using AmeisenBotX.Core.Engines.Event;
using AmeisenBotX.Core.Engines.Grinding;
using AmeisenBotX.Core.Engines.Jobs;
using AmeisenBotX.Core.Engines.Movement;
using AmeisenBotX.Core.Engines.Movement.Pathfinding;
using AmeisenBotX.Core.Engines.Movement.Settings;
using AmeisenBotX.Core.Engines.Quest;
using AmeisenBotX.Core.Engines.Tactic;
using AmeisenBotX.Memory;
using AmeisenBotX.RconClient;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Cache;
using AmeisenBotX.Wow.Combatlog;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        public IObjectProvider Objects => Wow.ObjectProvider;

        public IOffsetList Offsets { get; set; }

        public IPathfindingHandler PathfindingHandler { get; set; }

        public WowUnit Pet => Objects.Pet;

        public WowPlayer Player => Objects.Player;

        public IQuestEngine Quest { get; set; }

        public AmeisenBotRconClient Rcon { get; set; }

        public ITacticEngine Tactic { get; set; }

        public WowUnit Target => Objects.Target;

        public IWowInterface Wow { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<WowDynobject> GetAoeSpells(Vector3 position, bool onlyEnemy = true, float extends = 2.0f)
        {
            return Objects.WowObjects.OfType<WowDynobject>()
                .Where(e => e.Position.GetDistance(position) < e.Radius + extends
                         && (!onlyEnemy || Db.GetReaction(Player, GetWowObjectByGuid<WowUnit>(e.Caster)) == WowUnitReaction.Hostile));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowGameobject GetClosestGameobjectByDisplayId(Vector3 position, IEnumerable<int> displayIds)
        {
            return Objects.WowObjects.OfType<WowGameobject>()
                .Where(e => displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowUnit GetClosestQuestgiverByDisplayId(Vector3 position, IEnumerable<int> displayIds)
        {
            return Objects.WowObjects.OfType<WowUnit>()
                .Where(e => e.IsQuestgiver && !e.IsDead && displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowUnit GetClosestQuestgiverByNpcId(Vector3 position, IEnumerable<int> npcIds)
        {
            return Objects.WowObjects.OfType<WowUnit>()
                .Where(e => e.IsQuestgiver && !e.IsDead && npcIds.Contains(WowGuid.ToNpcId(e.Guid)))
                .OrderBy(e => e.Position.GetDistance(position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesInCombatWithParty<T>(Vector3 position, float distance) where T : WowUnit
        {
            return GetNearEnemies<T>(position, distance)                                // is hostile
                .Where(e => (e.IsInCombat && (e.IsTaggedByMe || !e.IsTaggedByOther))    // needs to be in combat and tagged by us or no one else
                         || e.TargetGuid == Player.Guid                                 // targets us
                         || Objects.Partymembers.Any(x => x.Guid == e.TargetGuid));     // targets a party member
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesInPath<T>(IEnumerable<Vector3> path, float distance) where T : WowUnit
        {
            foreach (Vector3 pathPosition in path)
            {
                IEnumerable<T> nearEnemies = GetNearEnemies<T>(pathPosition, distance);

                if (nearEnemies.Any())
                {
                    return nearEnemies;
                }
            }

            return Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesTargetingPartymembers<T>(Vector3 position, float distance) where T : WowUnit
        {
            return GetNearEnemies<T>(position, distance)                                // is hostile
                .Where(e => e.IsInCombat                                                // is in combat
                         && (Objects.Partymembers.Any(x => x.Guid == e.TargetGuid)      // is targeting a partymember
                            || Objects.PartyPets.Any(x => x.Guid == e.TargetGuid)));    // is targeting a pet in party
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearEnemies<T>(Vector3 position, float distance) where T : WowUnit
        {
            return Objects.WowObjects.OfType<T>()
                .Where(e => !e.IsDead && !e.IsNotAttackable                         // is alive and attackable
                         && Db.GetReaction(Player, e) == WowUnitReaction.Hostile    // is hostile
                         && e.Position.GetDistance(position) < distance);           // is in range
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearFriends<T>(Vector3 position, float distance) where T : WowUnit
        {
            return Objects.WowObjects.OfType<T>()
                .Where(e => !e.IsDead && !e.IsNotAttackable                         // is alive and attackable
                         && Db.GetReaction(Player, e) == WowUnitReaction.Friendly   // is hostile
                         && e.Position.GetDistance(position) < distance);           // is in range
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearPartymembers<T>(Vector3 position, float distance) where T : WowUnit
        {
            return Objects.Partymembers.OfType<T>()
                .Where(e => !e.IsDead && !e.IsNotAttackable                 // is alive and attackable
                         && e.Position.GetDistance(position) < distance);   // is in range
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetWowObjectByGuid<T>(ulong guid) where T : WowObject
        {
            return Objects.WowObjects.OfType<T>().FirstOrDefault(e => e.Guid == guid);
        }
    }
}