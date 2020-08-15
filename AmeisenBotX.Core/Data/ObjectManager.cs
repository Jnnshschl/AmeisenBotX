using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Cache.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data
{
    public class ObjectManager : IObjectManager
    {
        private readonly List<(IntPtr, WowObjectType)> objectPointers;
        private readonly object queryLock = new object();

        private readonly WowObject[] wowObjects;
        private IEnumerable<ulong> partymemberGuids;
        private IEnumerable<WowUnit> partymembers;
        private IEnumerable<ulong> partypetGuids;
        private IEnumerable<WowUnit> partypets;

        public ObjectManager(WowInterface wowInterface, AmeisenBotConfig config)
        {
            WowInterface = wowInterface;
            Config = config;

            partymemberGuids = new List<ulong>();
            partypetGuids = new List<ulong>();
            wowObjects = new WowObject[2048];
            objectPointers = new List<(IntPtr, WowObjectType)>(2048);
            partymembers = new List<WowUnit>();
            partypets = new List<WowUnit>();

            PoiCacheEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
        }

        public event ObjectUpdateComplete OnObjectUpdateComplete;

        public CameraInfo Camera { get; private set; }

        public string GameState { get; private set; }

        public bool IsWorldLoaded { get; private set; }

        public WowUnit LastTarget { get; private set; }

        public ulong LastTargetGuid { get; private set; }

        public MapId MapId { get; private set; }

        public int ObjectCount { get; set; }

        public WowUnit Partyleader { get; private set; }

        public ulong PartyleaderGuid { get; private set; }

        public IEnumerable<ulong> PartymemberGuids { get { lock (queryLock) { return partymemberGuids; } } }

        public IEnumerable<WowUnit> Partymembers { get { lock (queryLock) { return partymembers; } } }

        public IEnumerable<ulong> PartyPetGuids { get { lock (queryLock) { return partypetGuids; } } }

        public IEnumerable<WowUnit> PartyPets { get { lock (queryLock) { return partypets; } } }

        public WowUnit Pet { get; private set; }

        public ulong PetGuid { get; private set; }

        public WowPlayer Player { get; private set; }

        public IntPtr PlayerBase { get; private set; }

        public ulong PlayerGuid { get; private set; }

        public WowUnit Target { get; private set; }

        public ulong TargetGuid { get; private set; }

        public IEnumerable<WowObject> WowObjects { get { lock (queryLock) { return wowObjects[..ObjectCount]; } } }

        public int ZoneId { get; private set; }

        public string ZoneName { get; private set; }

        public string ZoneSubName { get; private set; }

        private AmeisenBotConfig Config { get; }

        private IntPtr CurrentObjectManager { get; set; }

        private TimegatedEvent PoiCacheEvent { get; }

        private WowInterface WowInterface { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowGameobject GetClosestWowGameobjectByDisplayId(IEnumerable<int> displayIds)
        {
            return WowObjects.OfType<WowGameobject>()
                .Where(e => displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowUnit GetClosestWowUnitByDisplayId(IEnumerable<int> displayIds, bool onlyQuestgiver = true)
        {
            return WowObjects.OfType<WowUnit>()
                .Where(e => (e.IsQuestgiver || !onlyQuestgiver) && displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesInCombatWithUs<T>(Vector3 position, double distance) where T : WowUnit
        {
            return GetNearEnemies<T>(position, distance)
                .Where(e => e.IsInCombat
                  && (PartymemberGuids.Contains(e.TargetGuid)
                      || PartyPetGuids.Contains(e.TargetGuid)
                      || e.TargetGuid == PlayerGuid
                      || e.IsTaggedByMe));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesTargetingPartymembers<T>(Vector3 position, double distance) where T : WowUnit
        {
            return GetNearEnemies<T>(position, distance)
                .Where(e => e.IsInCombat
                  && (PartymemberGuids.Contains(e.TargetGuid)
                      || PartyPetGuids.Contains(e.TargetGuid)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<WowDynobject> GetNearAoeSpells()
        {
            return WowObjects.OfType<WowDynobject>();
        }

        public IEnumerable<T> GetNearEnemies<T>(Vector3 position, double distance) where T : WowUnit
        {
            return WowObjects.OfType<T>()
                .Where(e => e != null
                         && e.Guid != PlayerGuid
                         && !e.IsDead
                         && !e.IsNotAttackable
                         && WowInterface.HookManager.GetUnitReaction(Player, e) != WowUnitReaction.Friendly
                         && e.Position.GetDistance(position) < distance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearFriends<T>(Vector3 position, double distance) where T : WowUnit
        {
            return WowObjects.OfType<T>()
                .Where(e => e != null
                         && e.Guid != PlayerGuid
                         && !e.IsDead
                         && !e.IsNotAttackable
                         && WowInterface.HookManager.GetUnitReaction(Player, e) == WowUnitReaction.Friendly
                         && e.Position.GetDistance(position) < distance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearPartymembers<T>(Vector3 position, double distance) where T : WowUnit
        {
            return WowObjects.OfType<T>()
                .Where(e => e != null
                         && e.Guid != PlayerGuid
                         && !e.IsDead
                         && !e.IsNotAttackable
                         && (PartymemberGuids.Contains(e.Guid) || PartyPetGuids.Contains(e.Guid))
                         && e.Position.GetDistance(position) < distance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<WowUnit> GetNearQuestgiverNpcs(Vector3 position, double distance)
        {
            return WowObjects.OfType<WowUnit>().Where(e => e.IsQuestgiver && e.Position.GetDistance(position) < distance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetWowObjectByGuid<T>(ulong guid) where T : WowObject
        {
            return WowObjects.OfType<T>().FirstOrDefault(e => e.Guid == guid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetWowUnitByName<T>(string name, StringComparison stringComparison = StringComparison.Ordinal) where T : WowUnit
        {
            return WowObjects.OfType<T>().FirstOrDefault(e => e.Name.Equals(name, stringComparison));
        }

        public void ProcessObject(int id, (IntPtr, WowObjectType) x)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(x.Item1, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress)
                && WowInterface.XMemory.Read(descriptorAddress, out ulong guid))
            {
                WowObject obj = WowObjects.FirstOrDefault(e => e?.Guid == guid);

                if (obj != null && obj.BaseAddress == x.Item1)
                {
                    obj.Update();
                }
                else
                {
                    wowObjects[id] = x.Item2 switch
                    {
                        WowObjectType.Container => new WowContainer(x.Item1, x.Item2, descriptorAddress),
                        WowObjectType.Corpse => new WowCorpse(x.Item1, x.Item2, descriptorAddress),
                        WowObjectType.Item => new WowItem(x.Item1, x.Item2, descriptorAddress),
                        WowObjectType.Dynobject => new WowDynobject(x.Item1, x.Item2, descriptorAddress),
                        WowObjectType.Gameobject => new WowGameobject(x.Item1, x.Item2, descriptorAddress),
                        WowObjectType.Player => new WowPlayer(x.Item1, x.Item2, descriptorAddress),
                        WowObjectType.Unit => new WowUnit(x.Item1, x.Item2, descriptorAddress),
                        _ => new WowObject(x.Item1, x.Item2, descriptorAddress),
                    };

                    wowObjects[id].Update();
                }

                if (wowObjects[id] != null && (x.Item2 == WowObjectType.Unit || x.Item2 == WowObjectType.Player))
                {
                    if (wowObjects[id].Guid == PlayerGuid)
                    {
                        if (WowInterface.XMemory.Read(WowInterface.OffsetList.ComboPoints, out byte comboPoints))
                        {
                            ((WowPlayer)wowObjects[id]).ComboPoints = comboPoints;
                        }

                        Player = (WowPlayer)wowObjects[id];
                    }

                    if (wowObjects[id].Guid == TargetGuid) { Target = (WowUnit)wowObjects[id]; }
                    if (wowObjects[id].Guid == PetGuid) { Pet = (WowUnit)wowObjects[id]; }
                    if (wowObjects[id].Guid == LastTargetGuid) { LastTarget = (WowUnit)wowObjects[id]; }
                    if (wowObjects[id].Guid == PartyleaderGuid) { Partyleader = (WowUnit)wowObjects[id]; }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefreshIsWorldLoaded()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                IsWorldLoaded = isWorldLoaded == 1;
                return IsWorldLoaded;
            }

            return false;
        }

        public void UpdateWowObjects()
        {
            IsWorldLoaded = UpdateGlobalVar<int>(WowInterface.OffsetList.IsWorldLoaded) == 1;

            if (!IsWorldLoaded) { return; }

            lock (queryLock)
            {
                PlayerGuid = UpdateGlobalVar<ulong>(WowInterface.OffsetList.PlayerGuid);
                TargetGuid = UpdateGlobalVar<ulong>(WowInterface.OffsetList.TargetGuid);
                LastTargetGuid = UpdateGlobalVar<ulong>(WowInterface.OffsetList.LastTargetGuid);
                PetGuid = UpdateGlobalVar<ulong>(WowInterface.OffsetList.PetGuid);
                PlayerBase = UpdateGlobalVar<IntPtr>(WowInterface.OffsetList.PlayerBase);
                MapId = UpdateGlobalVar<MapId>(WowInterface.OffsetList.MapId);
                ZoneId = UpdateGlobalVar<int>(WowInterface.OffsetList.ZoneId);
                GameState = UpdateGlobalVarString(WowInterface.OffsetList.GameState);

                if (WowInterface.XMemory.Read(WowInterface.OffsetList.CameraPointer, out IntPtr cameraPointer)
                    && WowInterface.XMemory.Read(IntPtr.Add(cameraPointer, (int)WowInterface.OffsetList.CameraOffset), out cameraPointer))
                {
                    Camera = UpdateGlobalVar<CameraInfo>(cameraPointer);
                }

                if (WowInterface.XMemory.Read(WowInterface.OffsetList.ZoneText, out IntPtr zoneNamePointer))
                {
                    ZoneName = UpdateGlobalVarString(zoneNamePointer);
                }

                if (WowInterface.XMemory.Read(WowInterface.OffsetList.ZoneSubText, out IntPtr zoneSubNamePointer))
                {
                    ZoneSubName = UpdateGlobalVarString(zoneSubNamePointer);
                }

                if (TargetGuid == 0) { Target = null; }
                if (PetGuid == 0) { Pet = null; }
                if (LastTargetGuid == 0) { LastTarget = null; }
                if (PartyleaderGuid == 0) { Partyleader = null; }

                WowInterface.XMemory.Read(WowInterface.OffsetList.ClientConnection, out IntPtr clientConnection);
                WowInterface.XMemory.Read(IntPtr.Add(clientConnection, (int)WowInterface.OffsetList.CurrentObjectManager), out IntPtr currentObjectManager);
                CurrentObjectManager = currentObjectManager;

                // read the first object
                WowInterface.XMemory.Read(IntPtr.Add(CurrentObjectManager, (int)WowInterface.OffsetList.FirstObject), out IntPtr activeObjectBaseAddress);
                WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.WowObjectType), out int activeObjectType);

                objectPointers.Clear();

                while (activeObjectType > 0 && activeObjectType < 8)
                {
                    objectPointers.Add((activeObjectBaseAddress, (WowObjectType)activeObjectType));

                    WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.NextObject), out activeObjectBaseAddress);
                    WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.WowObjectType), out activeObjectType);
                }

                ObjectCount = objectPointers.Count;

                for (int i = 0; i < ObjectCount; ++i)
                {
                    ProcessObject(i, objectPointers[i]);
                }

                // Parallel.For(0, objectPointers.Count, (x, y) => ProcessObject(x, objectPointers[x]));

                // read the party/raid leaders guid and if there is one, the group too
                PartyleaderGuid = ReadLeaderGuid();

                if (PartyleaderGuid > 0)
                {
                    partymemberGuids = ReadPartymemberGuids();
                    partymembers = wowObjects.OfType<WowUnit>().Where(e => e.Guid == PlayerGuid || partymemberGuids.Contains(e.Guid));

                    partypets = wowObjects.OfType<WowUnit>().Where(e => partymemberGuids.Contains(e.SummonedByGuid));
                    partypetGuids = partypets.Select(e => e.Guid);
                }
            }

            if (Config.CachePointsOfInterest && PoiCacheEvent.Run())
            {
                Task.Run(() => CachePois());
            }

            OnObjectUpdateComplete?.Invoke(WowObjects);
        }

        private void CachePois()
        {
            IEnumerable<WowGameobject> wowGameobjects = WowObjects.OfType<WowGameobject>();
            IEnumerable<WowUnit> wowUnits = WowObjects.OfType<WowUnit>();

            foreach (WowGameobject gameobject in wowGameobjects.Where(e => Enum.IsDefined(typeof(OreNodes), e.DisplayId)))
            {
                WowInterface.BotCache.CacheOre(MapId, (OreNodes)gameobject.DisplayId, gameobject.Position);
            }

            foreach (WowGameobject gameobject in wowGameobjects.Where(e => Enum.IsDefined(typeof(HerbNodes), e.DisplayId)))
            {
                WowInterface.BotCache.CacheHerb(MapId, (HerbNodes)gameobject.DisplayId, gameobject.Position);
            }

            foreach (WowGameobject gameobject in wowGameobjects.Where(e => e.GameobjectType == WowGameobjectType.Mailbox))
            {
                WowInterface.BotCache.CachePoi(MapId, PoiType.Mailbox, gameobject.Position);
            }

            foreach (WowUnit unit in wowUnits.Where(e => e.IsVendor))
            {
                WowInterface.BotCache.CachePoi(MapId, PoiType.Vendor, unit.Position);
            }

            foreach (WowUnit unit in wowUnits.Where(e => e.IsRepairVendor))
            {
                WowInterface.BotCache.CachePoi(MapId, PoiType.Repair, unit.Position);
            }
        }

        private ulong ReadLeaderGuid()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.RaidLeader, out ulong partyleaderGuid))
            {
                if (partyleaderGuid == 0
                    && WowInterface.XMemory.Read(WowInterface.OffsetList.PartyLeader, out partyleaderGuid))
                {
                    return partyleaderGuid;
                }

                return partyleaderGuid;
            }

            return 0;
        }

        private IEnumerable<ulong> ReadPartymemberGuids()
        {
            List<ulong> partymemberGuids = new List<ulong>();

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.PartyLeader, out ulong partyLeader)
                && partyLeader != 0
                && WowInterface.XMemory.Read(WowInterface.OffsetList.PartyPlayerGuids, out RawPartyGuids partyMembers))
            {
                partymemberGuids.AddRange(partyMembers.AsArray());
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.RaidLeader, out ulong raidLeader)
                && raidLeader != 0
                && WowInterface.XMemory.Read(WowInterface.OffsetList.RaidGroupStart, out RawRaidStruct raidStruct))
            {
                IEnumerable<IntPtr> raidPointers = raidStruct.GetPointers();
                ConcurrentBag<ulong> guids = new ConcurrentBag<ulong>();

                Parallel.ForEach(raidPointers, x =>
                {
                    if (WowInterface.XMemory.Read(x, out ulong guid) && guid != 0)
                    {
                        guids.Add(guid);
                    }
                });

                partymemberGuids.AddRange(guids);
            }

            return partymemberGuids.Where(e => e != 0).Distinct();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T UpdateGlobalVar<T>(IntPtr address) where T : unmanaged
        {
            return WowInterface.XMemory.Read(address, out T v) ? v : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string UpdateGlobalVarString(IntPtr address, int maxLenght = 128)
        {
            return WowInterface.XMemory.ReadString(address, Encoding.UTF8, out string v, maxLenght) ? v : string.Empty;
        }
    }
}