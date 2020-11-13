using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Cache.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Buffers;
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
        private readonly object queryLock = new object();
        private readonly (IntPtr, WowObjectType)[] WowObjectPointers;
        private WowObject[] wowObjects;

        public ObjectManager(WowInterface wowInterface, AmeisenBotConfig config)
        {
            WowInterface = wowInterface;
            Config = config;

            WowObjectPointers = new (IntPtr, WowObjectType)[2048];
            WowObjectPool = ArrayPool<WowObject>.Create(2048, 1);

            PartymemberGuids = new List<ulong>();
            PartyPetGuids = new List<ulong>();
            Partymembers = new List<WowUnit>();
            PartyPets = new List<WowUnit>();

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

        public List<ulong> PartymemberGuids { get; private set; }

        public List<WowUnit> Partymembers { get; private set; }

        public List<ulong> PartyPetGuids { get; private set; }

        public List<WowUnit> PartyPets { get; private set; }

        public WowUnit Pet { get; private set; }

        public ulong PetGuid { get; private set; }

        public WowPlayer Player { get; private set; }

        public IntPtr PlayerBase { get; private set; }

        public ulong PlayerGuid { get; private set; }

        public WowUnit Target { get; private set; }

        public ulong TargetGuid { get; private set; }

        public WowUnit Vehicle { get; private set; }

        public IEnumerable<WowObject> WowObjects { get { lock (queryLock) { return wowObjects; } } }

        public int ZoneId { get; private set; }

        public string ZoneName { get; private set; }

        public string ZoneSubName { get; private set; }

        private AmeisenBotConfig Config { get; }

        private bool PlayerGuidIsVehicle { get; set; }

        private TimegatedEvent PoiCacheEvent { get; }

        private WowInterface WowInterface { get; }

        private ArrayPool<WowObject> WowObjectPool { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowGameobject GetClosestWowGameobjectByDisplayId(IEnumerable<int> displayIds)
        {
            lock (queryLock)
            {
                return wowObjects.OfType<WowGameobject>()
                    .Where(e => displayIds.Contains(e.DisplayId))
                    .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                    .FirstOrDefault();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowUnit GetClosestWowUnitByDisplayId(IEnumerable<int> displayIds, bool onlyQuestgiver = true)
        {
            lock (queryLock)
            {
                return wowObjects.OfType<WowUnit>()
                        .Where(e => (e.IsQuestgiver || !onlyQuestgiver) && displayIds.Contains(e.DisplayId))
                        .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                        .FirstOrDefault();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> GetEnemiesInCombatWithUs<T>(Vector3 position, double distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return GetNearEnemies<T>(position, distance)
                    .Where(e => e.IsInCombat)
                    .ToList();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> GetEnemiesTargetingPartymembers<T>(Vector3 position, double distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return GetNearEnemies<T>(position, distance)
                    .Where(e => e.IsInCombat && (PartymemberGuids.Contains(e.TargetGuid) || PartyPetGuids.Contains(e.TargetGuid)))
                    .ToList();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<WowDynobject> GetNearAoeSpells()
        {
            lock (queryLock)
            {
                return wowObjects.OfType<WowDynobject>().ToList();
            }
        }

        public List<T> GetNearEnemies<T>(Vector3 position, double distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>()
                    .Where(e => !e.IsDead
                         && !e.IsNotAttackable
                         && WowInterface.HookManager.WowGetUnitReaction(Player, e) != WowUnitReaction.Friendly
                         && e.Position.GetDistance(position) < distance)
                    .ToList();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> GetNearFriends<T>(Vector3 position, double distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>()
                    .Where(e => !e.IsDead
                             && !e.IsNotAttackable
                             && WowInterface.HookManager.WowGetUnitReaction(Player, e) == WowUnitReaction.Friendly
                             && e.Position.GetDistance(position) < distance)
                    .ToList();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> GetNearPartymembers<T>(Vector3 position, double distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>()
                    .Where(e => !e.IsDead
                             && !e.IsNotAttackable
                             && (PartymemberGuids.Contains(e.Guid) || PartyPetGuids.Contains(e.Guid))
                             && e.Position.GetDistance(position) < distance)
                    .ToList();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<WowUnit> GetNearQuestgiverNpcs(Vector3 position, double distance)
        {
            lock (queryLock)
            {
                return wowObjects.OfType<WowUnit>().Where(e => e.IsQuestgiver && e.Position.GetDistance(position) < distance).ToList();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetWowObjectByGuid<T>(ulong guid) where T : WowObject
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>().FirstOrDefault(e => e.Guid == guid);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetWowUnitByName<T>(string name, StringComparison stringComparison = StringComparison.Ordinal) where T : WowUnit
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>().FirstOrDefault(e => e.Name.Equals(name, stringComparison));
            }
        }

        public WowObject ProcessObject(IntPtr ptr, WowObjectType type)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(ptr, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress))
            {
                WowObject obj = type switch
                {
                    WowObjectType.Container => new WowContainer(ptr, type, descriptorAddress),
                    WowObjectType.Corpse => new WowCorpse(ptr, type, descriptorAddress),
                    WowObjectType.Item => new WowItem(ptr, type, descriptorAddress),
                    WowObjectType.Dynobject => new WowDynobject(ptr, type, descriptorAddress),
                    WowObjectType.Gameobject => new WowGameobject(ptr, type, descriptorAddress),
                    WowObjectType.Player => new WowPlayer(ptr, type, descriptorAddress),
                    WowObjectType.Unit => new WowUnit(ptr, type, descriptorAddress),
                    _ => new WowObject(ptr, type, descriptorAddress),
                };

                obj.Update();

                if (obj != null && (type == WowObjectType.Unit || type == WowObjectType.Player))
                {
                    if (obj.Guid == PlayerGuid)
                    {
                        PlayerGuidIsVehicle = obj.GetType() != typeof(WowPlayer);

                        if (!PlayerGuidIsVehicle)
                        {
                            if (WowInterface.XMemory.Read(WowInterface.OffsetList.ComboPoints, out byte comboPoints))
                            {
                                ((WowPlayer)obj).ComboPoints = comboPoints;
                            }

                            Player = (WowPlayer)obj;
                            Vehicle = null;
                        }
                        else
                        {
                            Vehicle = (WowUnit)obj;
                        }
                    }

                    if (obj.Guid == TargetGuid) { Target = (WowUnit)obj; }
                    if (obj.Guid == PetGuid) { Pet = (WowUnit)obj; }
                    if (obj.Guid == LastTargetGuid) { LastTarget = (WowUnit)obj; }
                    if (obj.Guid == PartyleaderGuid) { Partyleader = (WowUnit)obj; }
                }

                return obj;
            }

            return null;
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

                // read the first object
                WowInterface.XMemory.Read(IntPtr.Add(currentObjectManager, (int)WowInterface.OffsetList.FirstObject), out IntPtr activeObjectBaseAddress);
                WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.WowObjectType), out int activeObjectType);

                int count = 0;

                while (activeObjectType > 0 && activeObjectType < 8)
                {
                    WowObjectPointers[count] = (activeObjectBaseAddress, (WowObjectType)activeObjectType);
                    ++count;

                    WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.NextObject), out activeObjectBaseAddress);
                    WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.WowObjectType), out activeObjectType);
                }

                ObjectCount = count;
                wowObjects = WowObjectPool.Rent(count);

                Parallel.For(0, count, (x, y) => wowObjects[x] = ProcessObject(WowObjectPointers[x].Item1, WowObjectPointers[x].Item2));

                if (PlayerGuidIsVehicle)
                {
                    // get the object with last known "good" guid
                    WowPlayer possiblePlayer = wowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.Guid == Player.Guid);

                    if (possiblePlayer != null)
                    {
                        Player = possiblePlayer;
                    }
                }

                // read the party/raid leaders guid and if there is one, the group too
                PartyleaderGuid = ReadLeaderGuid();

                if (PartyleaderGuid > 0)
                {
                    PartymemberGuids = ReadPartymemberGuids().ToList();
                    Partymembers = wowObjects.OfType<WowUnit>().Where(e => PartymemberGuids.Contains(e.Guid)).ToList();

                    PartyPets = wowObjects.OfType<WowUnit>().Where(e => PartymemberGuids.Contains(e.SummonedByGuid)).ToList();
                    PartyPetGuids = PartyPets.Select(e => e.Guid).ToList();
                }
            }

            if (Config.CachePointsOfInterest && PoiCacheEvent.Run())
            {
                Task.Run(() => CachePois());
            }

            OnObjectUpdateComplete?.Invoke(wowObjects);
        }

        private void CachePois()
        {
            IEnumerable<WowGameobject> wowGameobjects = wowObjects.OfType<WowGameobject>();
            IEnumerable<WowUnit> wowUnits = wowObjects.OfType<WowUnit>();

            foreach (WowGameobject gameobject in wowGameobjects.Where(e => Enum.IsDefined(typeof(OreNode), e.DisplayId)))
            {
                WowInterface.BotCache.CacheOre(MapId, (OreNode)gameobject.DisplayId, gameobject.Position);
            }

            foreach (WowGameobject gameobject in wowGameobjects.Where(e => Enum.IsDefined(typeof(HerbNode), e.DisplayId)))
            {
                WowInterface.BotCache.CacheHerb(MapId, (HerbNode)gameobject.DisplayId, gameobject.Position);
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