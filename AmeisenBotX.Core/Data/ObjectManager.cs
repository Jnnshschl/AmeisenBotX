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
        private readonly object queryLock = new object();
        private readonly ConcurrentBag<WowObject> wowObjects;

        private List<ulong> partymemberGuids;
        private List<WowUnit> partymembers;
        private List<ulong> partypetGuids;
        private List<WowUnit> partypets;

        public ObjectManager(WowInterface wowInterface, AmeisenBotConfig config)
        {
            WowInterface = wowInterface;
            Config = config;

            partymemberGuids = new List<ulong>();
            partypetGuids = new List<ulong>();
            wowObjects = new ConcurrentBag<WowObject>();
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

        public WowUnit Partyleader { get; private set; }

        public ulong PartyleaderGuid { get; private set; }

        public List<ulong> PartymemberGuids { get { lock (queryLock) { return partymemberGuids.ToList(); } } }

        public List<WowUnit> Partymembers { get { lock (queryLock) { return partymembers.ToList(); } } }

        public List<ulong> PartyPetGuids { get { lock (queryLock) { return partypetGuids.ToList(); } } }

        public List<WowUnit> PartyPets { get { lock (queryLock) { return partypets.ToList(); } } }

        public WowUnit Pet { get; private set; }

        public ulong PetGuid { get; private set; }

        public WowPlayer Player { get; private set; }

        public IntPtr PlayerBase { get; private set; }

        public ulong PlayerGuid { get; private set; }

        public WowUnit Target { get; private set; }

        public ulong TargetGuid { get; private set; }

        public List<WowObject> WowObjects { get { lock (queryLock) { return wowObjects.ToList(); } } }

        public int ZoneId { get; private set; }

        public string ZoneName { get; private set; }

        public string ZoneSubName { get; private set; }

        private AmeisenBotConfig Config { get; }

        private IntPtr CurrentObjectManager { get; set; }

        private TimegatedEvent PoiCacheEvent { get; }

        private WowInterface WowInterface { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowGameobject GetClosestWowGameobjectByDisplayId(List<int> displayIds)
        {
            return WowObjects.OfType<WowGameobject>()
                .Where(e => displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowUnit GetClosestWowUnitByDisplayId(List<int> displayIds, bool onlyQuestgiver = true)
        {
            return WowObjects.OfType<WowUnit>()
                .Where(e => (e.IsQuestgiver || !onlyQuestgiver) && displayIds.Contains(e.DisplayId))
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<WowUnit> GetEnemiesInCombatWithUs(Vector3 position, double distance)
        {
            return GetNearEnemies<WowUnit>(position, distance)
                .Where(e => e.IsInCombat
                  && (PartymemberGuids.Contains(e.TargetGuid)
                      || PartyPetGuids.Contains(e.TargetGuid)
                      || e.TargetGuid == PlayerGuid
                      || e.IsTaggedByMe))
                .ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<WowUnit> GetEnemiesTargetingPartymembers(Vector3 position, double distance)
        {
            return GetNearEnemies<WowUnit>(position, distance)
                .Where(e => e.IsInCombat
                  && (PartymemberGuids.Contains(e.TargetGuid)
                      || PartyPetGuids.Contains(e.TargetGuid)))
                .ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<WowDynobject> GetNearAoeSpells()
        {
            return WowObjects.OfType<WowDynobject>().ToList();
        }

        public List<T> GetNearEnemies<T>(Vector3 position, double distance) where T : WowUnit
        {
            return WowObjects.OfType<T>()
                .Where(e => e != null
                         && e.Guid != PlayerGuid
                         && !e.IsDead
                         && !e.IsNotAttackable
                         && WowInterface.HookManager.GetUnitReaction(Player, e) != WowUnitReaction.Friendly
                         && e.Position.GetDistance(position) < distance)
                .ToList();
        }

        public List<T> GetNearFriends<T>(Vector3 position, double distance) where T : WowUnit
        {
            return WowObjects.OfType<T>()
                .Where(e => e != null
                         && e.Guid != PlayerGuid
                         && !e.IsDead
                         && !e.IsNotAttackable
                         && WowInterface.HookManager.GetUnitReaction(Player, e) == WowUnitReaction.Friendly
                         && e.Position.GetDistance(position) < distance)
                .ToList();
        }

        public List<WowUnit> GetNearPartymembers(Vector3 position, double distance)
        {
            return WowObjects.OfType<WowUnit>()
                .Where(e => e != null
                         && e.Guid != PlayerGuid
                         && !e.IsDead
                         && !e.IsNotAttackable
                         && PartymemberGuids.Contains(e.Guid)
                         && e.Position.GetDistance(position) < distance)
                .ToList();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<WowUnit> GetNearQuestgiverNpcs(Vector3 position, double distance)
        {
            return WowObjects.OfType<WowUnit>().Where(e => e.IsQuestgiver && e.Position.GetDistance(position) < distance).ToList();
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

        public void ProcessObject((IntPtr, WowObjectType) x)
        {
            WowObject obj = x.Item2 switch
            {
                WowObjectType.Container => ReadWowContainer(x.Item1, x.Item2),
                WowObjectType.Corpse => ReadWowCorpse(x.Item1, x.Item2),
                WowObjectType.Item => ReadWowItem(x.Item1, x.Item2),
                WowObjectType.Dynobject => ReadWowDynobject(x.Item1, x.Item2),
                WowObjectType.Gameobject => ReadWowGameobject(x.Item1, x.Item2),
                WowObjectType.Player => ReadWowPlayer(x.Item1, x.Item2),
                WowObjectType.Unit => ReadWowUnit(x.Item1, x.Item2),
                _ => ReadWowObject(x.Item1, x.Item2),
            };

            if (obj != null)
            {
                wowObjects.Add(obj);
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

                WowInterface.XMemory.Read(WowInterface.OffsetList.ClientConnection, out IntPtr clientConnection);
                WowInterface.XMemory.Read(IntPtr.Add(clientConnection, (int)WowInterface.OffsetList.CurrentObjectManager), out IntPtr currentObjectManager);
                CurrentObjectManager = currentObjectManager;

                // read the first object
                WowInterface.XMemory.Read(IntPtr.Add(CurrentObjectManager, (int)WowInterface.OffsetList.FirstObject), out IntPtr activeObjectBaseAddress);
                WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.WowObjectType), out int activeObjectType);

                List<(IntPtr, WowObjectType)> objectPointers = new List<(IntPtr, WowObjectType)>();

                while (Enum.IsDefined(typeof(WowObjectType), activeObjectType))
                {
                    objectPointers.Add((activeObjectBaseAddress, (WowObjectType)activeObjectType));

                    WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.NextObject), out activeObjectBaseAddress);
                    WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.WowObjectType), out activeObjectType);
                }

                wowObjects.Clear();

                Parallel.ForEach(objectPointers, ProcessObject);

                // read the party/raid leaders guid and if there is one, the group too
                PartyleaderGuid = ReadLeaderGuid();

                if (PartyleaderGuid > 0)
                {
                    partymemberGuids = ReadPartymemberGuids();
                    partymembers = wowObjects.OfType<WowUnit>().Where(e => e.Guid == PlayerGuid || PartymemberGuids.Contains(e.Guid)).ToList();

                    partypets = wowObjects.OfType<WowUnit>().Where(e => PartymemberGuids.Contains(e.SummonedByGuid)).ToList();
                    partypetGuids = partypets.Select(e => e.Guid).ToList();
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
            IEnumerable<WowObject> wowObjects = WowObjects.ToList();
            IEnumerable<WowGameobject> wowGameobjects = wowObjects.OfType<WowGameobject>();
            IEnumerable<WowUnit> wowUnits = wowObjects.OfType<WowUnit>();

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

        private List<ulong> ReadPartymemberGuids()
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
                List<IntPtr> raidPointers = raidStruct.GetPointers();
                ConcurrentBag<ulong> guids = new ConcurrentBag<ulong>();

                Parallel.ForEach(raidPointers, x =>
                {
                    if (WowInterface.XMemory.Read(x, out ulong guid) && guid != 0)
                    {
                        guids.Add(guid);
                    }
                });

                partymemberGuids.AddRange(guids.ToList());
            }

            return partymemberGuids.Where(e => e != 0).Distinct().ToList();
        }

        private string ReadPlayerName(ulong guid)
        {
            if (WowInterface.BotCache.TryGetUnitName(guid, out string cachedName))
            {
                return cachedName;
            }

            uint shortGuid;
            uint offset;

            WowInterface.XMemory.Read(IntPtr.Add(WowInterface.OffsetList.NameStore, (int)WowInterface.OffsetList.NameMask), out uint nameMask);
            WowInterface.XMemory.Read(IntPtr.Add(WowInterface.OffsetList.NameStore, (int)WowInterface.OffsetList.NameBase), out uint nameBase);

            shortGuid = (uint)guid & 0xfffffff;
            offset = 12 * (nameMask & shortGuid);

            WowInterface.XMemory.Read(new IntPtr(nameBase + offset + 8), out uint current);
            WowInterface.XMemory.Read(new IntPtr(nameBase + offset), out offset);

            if ((current & 0x1) == 0x1)
            {
                return string.Empty;
            }

            WowInterface.XMemory.Read(new IntPtr(current), out uint testGuid);

            while (testGuid != shortGuid)
            {
                WowInterface.XMemory.Read(new IntPtr(current + offset + 4), out current);

                if ((current & 0x1) == 0x1)
                {
                    return string.Empty;
                }

                WowInterface.XMemory.Read(new IntPtr(current), out testGuid);
            }

            WowInterface.XMemory.ReadString(new IntPtr(current + (int)WowInterface.OffsetList.NameString), Encoding.UTF8, out string name, 16);

            if (name.Length > 0)
            {
                WowInterface.BotCache.CacheName(guid, name);
            }

            return name;
        }

        private string ReadUnitName(IntPtr activeObject, ulong guid)
        {
            if (WowInterface.BotCache.TryGetUnitName(guid, out string cachedName))
            {
                return cachedName;
            }

            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowUnitName1), out IntPtr objName)
                && WowInterface.XMemory.Read(IntPtr.Add(objName, (int)WowInterface.OffsetList.WowUnitName2), out objName)
                && WowInterface.XMemory.ReadString(objName, Encoding.UTF8, out string name))
            {
                if (name.Length > 0)
                {
                    WowInterface.BotCache.CacheName(guid, name);
                }

                return name;
            }

            return "unknown";
        }

        private WowContainer ReadWowContainer(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectPosition), out Vector3 position))
            {
                WowContainer container = new WowContainer(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position
                };

                return container.UpdateRawWowContainer(WowInterface.XMemory);
            }

            return null;
        }

        private WowCorpse ReadWowCorpse(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectPosition), out Vector3 position))
            {
                WowCorpse corpse = new WowCorpse(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position
                };

                return corpse.UpdateRawWowCorpse(WowInterface.XMemory);
            }

            return null;
        }

        private WowDynobject ReadWowDynobject(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectPosition), out Vector3 position))
            {
                WowDynobject dynobject = new WowDynobject(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position
                };

                return dynobject.UpdateRawWowDynobject(WowInterface.XMemory);
            }

            return null;
        }

        private WowGameobject ReadWowGameobject(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowGameobjectPosition), out Vector3 position))
            {
                WowGameobject gameobject = new WowGameobject(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position
                };

                return gameobject.UpdateRawWowGameobject(WowInterface.XMemory);
            }

            return null;
        }

        private WowItem ReadWowItem(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectPosition), out Vector3 position))
            {
                WowItem item = new WowItem(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position
                };

                return item.UpdateRawWowItem(WowInterface.XMemory);
            }

            return null;
        }

        private WowObject ReadWowObject(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectPosition), out Vector3 position))
            {
                WowObject obj = new WowObject(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position
                };

                return obj.UpdateRawWowObject(WowInterface.XMemory);
            }

            return null;
        }

        private WowPlayer ReadWowPlayer(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress))
            {
                WowPlayer player = new WowPlayer(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress
                };

                // first read the descriptor, then lookup the Name by GUID
                player.UpdateRawWowPlayer(WowInterface.XMemory);

                player.Name = ReadPlayerName(player.Guid);
                player.Auras = WowInterface.HookManager.GetUnitAuras(player);

                if (WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowUnitPosition), out Vector3 position))
                {
                    player.Position = position;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowUnitRotation), out float rotation))
                {
                    player.Rotation = rotation;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowUnitIsAutoAttacking), out int isAutoAttacking))
                {
                    player.IsAutoAttacking = isAutoAttacking == 1;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.CurrentlyCastingSpellId), out int castingId))
                {
                    player.CurrentlyCastingSpellId = castingId;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.CurrentlyChannelingSpellId), out int channelingId))
                {
                    player.CurrentlyChannelingSpellId = channelingId;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowUnitSwimFlags), out uint swimFlags))
                {
                    player.IsSwimming = (swimFlags & 0x200000) != 0;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowUnitFlyFlagsPointer), out IntPtr flyFlagsPointer)
                && WowInterface.XMemory.Read(IntPtr.Add(flyFlagsPointer, (int)WowInterface.OffsetList.WowUnitFlyFlags), out uint flyFlags))
                {
                    player.IsFlying = (flyFlags & 0x2000000) != 0;
                }

                if (WowInterface.XMemory.Read(WowInterface.OffsetList.BreathTimer, out int breathTimer))
                {
                    player.IsUnderwater = breathTimer > 0;
                }

                if (player.Guid == PlayerGuid)
                {
                    if (WowInterface.XMemory.Read(WowInterface.OffsetList.ComboPoints, out byte comboPoints))
                    {
                        player.ComboPoints = comboPoints;
                    }

                    Player = player;
                }

                if (player.Guid == TargetGuid) { Target = player; }
                if (player.Guid == PetGuid) { Pet = player; }
                if (player.Guid == LastTargetGuid) { LastTarget = player; }
                if (player.Guid == PartyleaderGuid) { Partyleader = player; }

                return player;
            }

            return null;
        }

        private WowUnit ReadWowUnit(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress))
            {
                WowUnit unit = new WowUnit(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                };

                // First read the descriptor, then lookup the Name by GUID
                unit.UpdateRawWowUnit(WowInterface.XMemory);

                unit.Name = ReadUnitName(activeObject, unit.Guid);
                unit.Auras = WowInterface.HookManager.GetUnitAuras(unit);

                if (WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowUnitPosition), out Vector3 position))
                {
                    unit.Position = position;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowUnitRotation), out float rotation))
                {
                    unit.Rotation = rotation;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.WowUnitIsAutoAttacking), out int isAutoAttacking))
                {
                    unit.IsAutoAttacking = isAutoAttacking == 1;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.CurrentlyCastingSpellId), out int castingId))
                {
                    unit.CurrentlyCastingSpellId = castingId;
                }

                if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, (int)WowInterface.OffsetList.CurrentlyChannelingSpellId), out int channelingId))
                {
                    unit.CurrentlyChannelingSpellId = channelingId;
                }

                if (unit.Guid == TargetGuid) { Target = unit; }
                if (unit.Guid == PetGuid) { Pet = unit; }
                if (unit.Guid == LastTargetGuid) { LastTarget = unit; }
                if (unit.Guid == PartyleaderGuid) { Partyleader = unit; }

                return unit;
            }

            return null;
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