using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
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

        CameraInfo Camera { get; }

        string GameState { get; }

        bool IsWorldLoaded { get; }

        WowUnit LastTarget { get; }

        WowUnit Partyleader { get; }

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

        WowGameobject GetClosestWowGameobjectByDisplayId(List<int> displayIds);

        WowUnit GetClosestWowUnitByDisplayId(List<int> displayIds, bool onlyQuestgiver = true);

        List<WowUnit> GetEnemiesTargetingPartymembers(Vector3 position, double distance);

        List<WowDynobject> GetNearAoeSpells();

        List<T> GetNearEnemies<T>(Vector3 position, double distance) where T : WowUnit;

        List<T> GetNearFriends<T>(Vector3 position, double distance) where T : WowUnit;

        List<WowPlayer> GetNearPartymembers(Vector3 position, double distance);

        T GetWowObjectByGuid<T>(ulong guid) where T : WowObject;

        WowPlayer GetWowPlayerByName(string playername, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase);

        bool RefreshIsWorldLoaded();

        void UpdateObject<T>(T wowObject) where T : WowObject;

        void UpdateObject(WowObjectType wowObjectType, IntPtr baseAddress);

        void UpdateWowObjects();
    }
}