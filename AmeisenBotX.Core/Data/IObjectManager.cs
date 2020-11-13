using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data
{
    public delegate void ObjectUpdateComplete(IEnumerable<WowObject> wowObjects);

    public interface IObjectManager
    {
        event ObjectUpdateComplete OnObjectUpdateComplete;

        CameraInfo Camera { get; }

        string GameState { get; }

        bool IsWorldLoaded { get; }

        WowUnit LastTarget { get; }

        ulong LastTargetGuid { get; }

        MapId MapId { get; }

        int ObjectCount { get; }

        WowUnit Partyleader { get; }

        ulong PartyleaderGuid { get; }

        List<ulong> PartymemberGuids { get; }

        List<WowUnit> Partymembers { get; }

        List<ulong> PartyPetGuids { get; }

        List<WowUnit> PartyPets { get; }

        WowUnit Pet { get; }

        ulong PetGuid { get; }

        WowPlayer Player { get; }

        IntPtr PlayerBase { get; }

        ulong PlayerGuid { get; }

        WowUnit Target { get; }

        ulong TargetGuid { get; }

        WowUnit Vehicle { get; }

        IEnumerable<WowObject> WowObjects { get; }

        int ZoneId { get; }

        string ZoneName { get; }

        string ZoneSubName { get; }

        WowGameobject GetClosestWowGameobjectByDisplayId(IEnumerable<int> displayIds);

        WowUnit GetClosestWowUnitByDisplayId(IEnumerable<int> displayIds, bool onlyQuestgiver = true);

        List<T> GetEnemiesInCombatWithUs<T>(Vector3 position, double distance) where T : WowUnit;

        List<T> GetEnemiesTargetingPartymembers<T>(Vector3 position, double distance) where T : WowUnit;

        List<WowDynobject> GetNearAoeSpells();

        List<T> GetNearEnemies<T>(Vector3 position, double distance) where T : WowUnit;

        List<T> GetNearFriends<T>(Vector3 position, double distance) where T : WowUnit;

        List<T> GetNearPartymembers<T>(Vector3 position, double distance) where T : WowUnit;

        List<WowUnit> GetNearQuestgiverNpcs(Vector3 position, double distance);

        T GetWowObjectByGuid<T>(ulong guid) where T : WowObject;

        T GetWowUnitByName<T>(string name, StringComparison stringComparison = StringComparison.Ordinal) where T : WowUnit;

        bool RefreshIsWorldLoaded();

        void UpdateWowObjects();
    }
}