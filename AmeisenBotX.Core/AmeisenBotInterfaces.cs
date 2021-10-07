using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Storage;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Battleground;
using AmeisenBotX.Core.Engines.Combat.Classes;
using AmeisenBotX.Core.Engines.Dungeon;
using AmeisenBotX.Core.Engines.Grinding;
using AmeisenBotX.Core.Engines.Jobs;
using AmeisenBotX.Core.Engines.Movement;
using AmeisenBotX.Core.Engines.Movement.Pathfinding;
using AmeisenBotX.Core.Engines.PvP;
using AmeisenBotX.Core.Engines.Quest;
using AmeisenBotX.Core.Engines.Tactic;
using AmeisenBotX.Core.Engines.Test;
using AmeisenBotX.Core.Logic.Idle;
using AmeisenBotX.Core.Managers.Character;
using AmeisenBotX.Core.Managers.Chat;
using AmeisenBotX.Core.Managers.Threat;
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

        public ICombatlogParser CombatLog { get; set; }

        public IAmeisenBotDb Db { get; set; }

        public IDungeonEngine Dungeon { get; set; }

        public IGrindingEngine Grinding { get; set; }

        public IdleActionManager IdleActions { get; set; }

        public IJobEngine Jobs { get; set; }

        public IWowUnit LastTarget => Objects.LastTarget;

        public IMemoryApi Memory { get; set; }

        public IMovementEngine Movement { get; set; }

        public IObjectProvider Objects => Wow.ObjectProvider;

        public IPathfindingHandler PathfindingHandler { get; set; }

        public IWowUnit Pet => Objects.Pet;

        public IWowPlayer Player => Objects.Player;

        public IPvpEngine Pvp { get; set; }

        public IQuestEngine Quest { get; set; }

        public AmeisenBotRconClient Rcon { get; set; }

        public StorageManager Storage { get; set; }

        public ITacticEngine Tactic { get; set; }

        public IWowUnit Target => Objects.Target;

        public ITestEngine Test { get; set; }

        public ThreatManager Threat { get; set; }

        public IWowInterface Wow { get; set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<IWowDynobject> GetAoeSpells(Vector3 position, bool onlyEnemy = false, float extends = 2.0f)
        {
            return Objects.WowObjects.OfType<IWowDynobject>()
                .Where(e => e.Position.GetDistance(position) < e.Radius + extends
                         && (!onlyEnemy || Db.GetReaction(Player, GetWowObjectByGuid<IWowUnit>(e.Caster)) == WowUnitReaction.Hostile));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IWowGameobject GetClosestGameObjectByDisplayId(Vector3 position, IEnumerable<int> displayIds)
        {
            return Objects.WowObjects.OfType<IWowGameobject>()
                .Where(e => displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IWowUnit GetClosestQuestGiverByDisplayId(Vector3 position, IEnumerable<int> displayIds, bool onlyQuestGivers = true)
        {
            return Objects.WowObjects.OfType<IWowUnit>()
                .Where(e => !e.IsDead && (!onlyQuestGivers || e.IsQuestgiver) && displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IWowUnit GetClosestQuestGiverByNpcId(Vector3 position, IEnumerable<int> npcIds, bool onlyQuestGivers = true)
        {
            return Objects.WowObjects.OfType<IWowUnit>()
                .Where(e => !e.IsDead && (!onlyQuestGivers || e.IsQuestgiver) && npcIds.Contains(BotUtils.GuidToNpcId(e.Guid)))
                .OrderBy(e => e.Position.GetDistance(position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IWowUnit GetClosestTrainerByEntryId(int entryId)
        {
            return Objects.WowObjects.OfType<IWowUnit>()
                .Where(e => !e.IsDead && e.IsTrainer && Db.GetReaction(Player, e) != WowUnitReaction.Hostile && e.EntryId == entryId)
                .OrderBy(e => e.Position.GetDistance(Player.Position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IWowUnit GetClosestVendorByEntryId(int entryId)
        {
            return Objects.WowObjects.OfType<IWowUnit>()
                .Where(e => !e.IsDead && e.IsVendor && Db.GetReaction(Player, e) != WowUnitReaction.Hostile && e.EntryId == entryId)
                .OrderBy(e => e.Position.GetDistance(Player.Position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesInCombatWithMe<T>(Vector3 position, float distance)
            where T : IWowUnit
        {
            return GetNearEnemies<T>(position, distance)  // is hostile
                .Where(e => e.IsInCombat                  // needs to be in combat
                         && e.TargetGuid == Player.Guid); // targets us
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesInCombatWithParty<T>(Vector3 position, float distance)
            where T : IWowUnit
        {
            return GetNearEnemies<T>(position, distance)                            // is hostile
                .Where(e => e.IsInCombat && (e.IsTaggedByMe || !e.IsTaggedByOther)  // needs to be in combat and tagged by us or no one else
                         || e.TargetGuid == Player.Guid                             // targets us
                         || Objects.Partymembers.Any(x => x.Guid == e.TargetGuid)); // targets a party member
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesInPath<T>(IEnumerable<Vector3> path, float distance)
            where T : IWowUnit
        {
            foreach (Vector3 pathPosition in path)
            {
                IEnumerable<T> nearEnemies = GetNearEnemies<T>(pathPosition, distance)
                    .ToArray();

                if (nearEnemies.Any()) return nearEnemies;
            }

            return Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesOrNeutralsInCombatWithMe<T>(Vector3 position, float distance)
            where T : IWowUnit
        {
            return GetNearEnemiesOrNeutrals<T>(position, distance) // is hostile/neutral
                .Where(e => e.IsInCombat                           // needs to be in combat
                         && e.TargetGuid == Player.Guid);          // targets us
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesTargetingMe<T>(Vector3 position, float distance)
            where T : IWowUnit
        {
            return GetNearEnemies<T>(position, distance)  // is hostile
                .Where(e => e.TargetGuid == Player.Guid); // targets us
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesTargetingPartyMembers<T>(Vector3 position, float distance)
            where T : IWowUnit
        {
            return GetNearEnemies<T>(position, distance)                           // is hostile
                .Where(e => e.IsInCombat                                           // is in combat
                         && (Objects.Partymembers.Any(x => x.Guid == e.TargetGuid) // is targeting a partymember
                         || Objects.PartyPets.Any(x => x.Guid == e.TargetGuid)));  // is targeting a pet in party
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearEnemies<T>(Vector3 position, float distance)
            where T : IWowUnit
        {
            return Objects.WowObjects.OfType<T>()
                .Where(e => !e.IsDead && !e.IsNotAttackable                      // is alive and attackable
                         && Db.GetReaction(Player, e) == WowUnitReaction.Hostile // is hostile
                         && e.Position.GetDistance(position) < distance);        // is in range
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearEnemiesOrNeutrals<T>(Vector3 position, float distance)
            where T : IWowUnit
        {
            return Objects.WowObjects.OfType<T>()
                .Where(e => !e.IsDead && !e.IsNotAttackable                       // is alive and attackable
                         && Db.GetReaction(Player, e) != WowUnitReaction.Friendly // is hostile/neutral
                         && e.Position.GetDistance(position) < distance);         // is in range
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearFriends<T>(Vector3 position, float distance)
            where T : IWowUnit
        {
            return Objects.WowObjects.OfType<T>()
                .Where(e => !e.IsDead && !e.IsNotAttackable                       // is alive and attackable
                         && Db.GetReaction(Player, e) == WowUnitReaction.Friendly // is hostile
                         && e.Position.GetDistance(position) < distance);         // is in range
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearPartyMembers<T>(Vector3 position, float distance)
            where T : IWowUnit
        {
            return Objects.Partymembers.OfType<T>()
                .Where(e => !e.IsDead && !e.IsNotAttackable               // is alive and attackable
                         && e.Position.GetDistance(position) < distance); // is in range
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetWowObjectByGuid<T>(ulong guid)
            where T : IWowObject
        {
            return Objects.WowObjects.OfType<T>().FirstOrDefault(e => e.Guid == guid);
        }
    }
}