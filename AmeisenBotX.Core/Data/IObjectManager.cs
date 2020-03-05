using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data
{
    public delegate void ObjectUpdateComplete(List<WowObject> wowObjects);

    public interface IObjectManager
    {
        event ObjectUpdateComplete OnObjectUpdateComplete;

        GameState GameState { get; }

        bool IsWorldLoaded { get; }

        WowUnit LastTarget { get; }

        ulong LastTargetGuid { get; }

        MapId MapId { get; }

        ulong PartyleaderGuid { get; }

        List<ulong> PartymemberGuids { get; }

        List<WowObject> Partymembers { get; }

        WowUnit Pet { get; }

        ulong PetGuid { get; }

        WowPlayer Player { get; }

        IntPtr PlayerBase { get; }

        ulong PlayerGuid { get; }

        WowUnit Target { get; }

        ulong TargetGuid { get; }

        List<WowObject> WowObjects { get; }

        int ZoneId { get; }

        public IEnumerable<WowPlayer> GetNearEnemies(Vector3 position, double distance);

        public IEnumerable<WowPlayer> GetNearFriends(Vector3 position, int distance);

        public WowObject GetWowObjectByGuid(ulong guid);

        public T GetWowObjectByGuid<T>(ulong guid) where T : WowObject;

        public WowPlayer GetWowPlayerByName(string playername, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase);

        public ulong ReadPartyLeaderGuid();

        public List<ulong> ReadPartymemberGuids();

        public WowObject ReadWowObject(IntPtr activeObject, WowObjectType wowObjectType = WowObjectType.None);

        public WowPlayer ReadWowPlayer(IntPtr activeObject, WowObjectType wowObjectType = WowObjectType.Player);

        public WowUnit ReadWowUnit(IntPtr activeObject, WowObjectType wowObjectType = WowObjectType.Unit);

        public bool RefreshIsWorldLoaded();

        public void UpdateObject<T>(T wowObject) where T : WowObject;

        public void UpdateObject(WowObjectType wowObjectType, IntPtr baseAddress);

        public void UpdateWowObjects();
    }
}