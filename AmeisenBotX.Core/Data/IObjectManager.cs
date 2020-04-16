using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data
{
    public delegate void ObjectUpdateComplete(List<WowObject> wowObjects);

    public interface IObjectManager
    {
        event ObjectUpdateComplete OnObjectUpdateComplete;

        string GameState { get; }

        bool IsWorldLoaded { get; }

        WowUnit LastTarget { get; }

        ulong LastTargetGuid { get; }

        MapId MapId { get; }

        ulong PartyleaderGuid { get; }

        List<ulong> PartymemberGuids { get; }

        List<WowUnit> Partymembers { get; }

        WowUnit Pet { get; }

        ulong PetGuid { get; }

        WowPlayer Player { get; }

        IntPtr PlayerBase { get; }

        ulong PlayerGuid { get; }

        WowUnit Target { get; }

        ulong TargetGuid { get; }

        List<WowObject> WowObjects { get; }

        int ZoneId { get; }

        string ZoneName { get; }

        string ZoneSubName { get; }

        IEnumerable<T> GetNearEnemies<T>(Vector3 position, double distance) where T : WowUnit;

        IEnumerable<T> GetNearFriends<T>(Vector3 position, double distance) where T : WowUnit;

        T GetWowObjectByGuid<T>(ulong guid) where T : WowObject;

        WowPlayer GetWowPlayerByName(string playername, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase);

        bool RefreshIsWorldLoaded();

        void UpdateObject<T>(T wowObject) where T : WowObject;

        void UpdateObject(WowObjectType wowObjectType, IntPtr baseAddress);

        void UpdateWowObjects();

        IEnumerable<WowPlayer> GetNearPartymembers(Vector3 position, double distance);

        IEnumerable<WowUnit> GetEnemiesTargetingPartymembers(Vector3 position, double distance);
    }
}