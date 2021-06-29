using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AmeisenBotX.Wow.Objects
{
    public interface IObjectProvider
    {
        event Action<IEnumerable<WowObject>> OnObjectUpdateComplete;

        RawCameraInfo Camera { get; }

        string GameState { get; }

        bool IsTargetInLineOfSight { get; }

        bool IsWorldLoaded { get; }

        WowUnit LastTarget { get; }

        WowMapId MapId { get; }

        Vector3 MeanGroupPosition { get; }

        int ObjectCount { get; set; }

        WowUnit Partyleader { get; }

        IEnumerable<ulong> PartymemberGuids { get; }

        IEnumerable<WowUnit> Partymembers { get; }

        IEnumerable<ulong> PartyPetGuids { get; }

        IEnumerable<WowUnit> PartyPets { get; }

        WowUnit Pet { get; }

        WowPlayer Player { get; }

        IntPtr PlayerBase { get; }

        WowUnit Target { get; }

        WowUnit Vehicle { get; }

        IEnumerable<WowObject> WowObjects { get; }

        int ZoneId { get; }

        string ZoneName { get; }

        string ZoneSubName { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<WowDynobject> GetAoeSpells(Func<WowUnit, WowUnit, WowUnitReaction> pred, Vector3 position, bool onlyEnemy = true, float extends = 2.0f)
        {
            return WowObjects.OfType<WowDynobject>()
                .Where(e => e.Position.GetDistance(position) < e.Radius + extends
                && (!onlyEnemy || pred(Player, GetWowObjectByGuid<WowUnit>(e.Caster)) != WowUnitReaction.Friendly));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowGameobject GetClosestWowGameobjectByDisplayId(Vector3 position, IEnumerable<int> displayIds)
        {
            return WowObjects.OfType<WowGameobject>()
                .Where(e => displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowUnit GetClosestWowUnitByDisplayId(Vector3 position, IEnumerable<int> displayIds, bool onlyQuestgiver = true)
        {
            return WowObjects.OfType<WowUnit>()
                .Where(e => (e.IsQuestgiver || !onlyQuestgiver) && displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowUnit GetClosestWowUnitByNpcId(Vector3 position, IEnumerable<int> npcIds, bool onlyQuestgiver = true)
        {
            return WowObjects.OfType<WowUnit>()
                .Where(e => (e.IsQuestgiver || !onlyQuestgiver) && npcIds.Contains(WowGuid.ToNpcId(e.Guid)))
                .OrderBy(e => e.Position.GetDistance(position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesInCombatWithParty<T>(Func<WowUnit, WowUnit, WowUnitReaction> pred, Vector3 position, float distance) where T : WowUnit
        {
            return GetNearEnemies<T>(pred, position, distance)
                .Where(e => e.IsInCombat && (e.IsTaggedByMe || e.TargetGuid == Player.Guid || Partymembers.Any(e => e.Guid == e.Guid)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesInPath<T>(Func<WowUnit, WowUnit, WowUnitReaction> pred, IEnumerable<Vector3> path, float distance) where T : WowUnit
        {
            foreach (Vector3 pathPosition in path)
            {
                IEnumerable<T> nearEnemies = GetNearEnemies<T>(pred, pathPosition, distance);

                if (nearEnemies.Any())
                {
                    return nearEnemies;
                }
            }

            return Array.Empty<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesTargetingPartymembers<T>(Func<WowUnit, WowUnit, WowUnitReaction> pred, Vector3 position, float distance) where T : WowUnit
        {
            return GetNearEnemies<T>(pred, position, distance)
                .Where(e => e.IsInCombat && (PartymemberGuids.Contains(e.TargetGuid) || PartyPetGuids.Contains(e.TargetGuid)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearEnemies<T>(Func<WowUnit, WowUnit, WowUnitReaction> pred, Vector3 position, float distance) where T : WowUnit
        {
            return WowObjects.OfType<T>()
                .Where(e => !e.IsDead
                     && !e.IsNotAttackable
                     && pred(Player, e) == WowUnitReaction.Hostile
                     && e.Position.GetDistance(position) < distance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearFriends<T>(Func<WowUnit, WowUnit, WowUnitReaction> pred, Vector3 position, float distance) where T : WowUnit
        {
            return WowObjects.OfType<T>()
                .Where(e => !e.IsDead
                         && !e.IsNotAttackable
                         && pred(Player, e) == WowUnitReaction.Friendly
                         && e.Position.GetDistance(position) < distance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearPartymembers<T>(Vector3 position, float distance) where T : WowUnit
        {
            return WowObjects.OfType<T>()
                .Where(e => !e.IsDead
                         && !e.IsNotAttackable
                         && (PartymemberGuids.Contains(e.Guid) || PartyPetGuids.Contains(e.Guid))
                         && e.Position.GetDistance(position) < distance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetWowObjectByGuid<T>(ulong guid) where T : WowObject
        {
            return WowObjects.OfType<T>().FirstOrDefault(e => e.Guid == guid);
        }
    }
}