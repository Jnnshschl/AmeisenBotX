using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Db.Enums;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.Raw;
using AmeisenBotX.Core.Hook.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Personality.Enums;
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
        private const int MAX_OBJECT_COUNT = 4096;

        private readonly object queryLock = new();

        private readonly IntPtr[] wowObjectPointers;
        private readonly WowObject[] wowObjects;

        public ObjectManager(WowInterface wowInterface, AmeisenBotConfig config)
        {
            WowInterface = wowInterface;
            Config = config;

            wowObjectPointers = new IntPtr[MAX_OBJECT_COUNT];
            wowObjects = new WowObject[MAX_OBJECT_COUNT];

            PartymemberGuids = new List<ulong>();
            PartyPetGuids = new List<ulong>();
            Partymembers = new List<WowUnit>();
            PartyPets = new List<WowUnit>();

            PoiCacheEvent = new(TimeSpan.FromSeconds(2));
            RelationshipEvent = new(TimeSpan.FromSeconds(2));

            WowInterface.HookManager.OnGameInfoPush += HookManagerOnGameInfoPush;
        }

        ///<inheritdoc cref="IObjectManager.OnObjectUpdateComplete"/>
        public event Action<IEnumerable<WowObject>> OnObjectUpdateComplete;

        ///<inheritdoc cref="IObjectManager.Camera"/>
        public RawCameraInfo Camera { get; private set; }

        ///<inheritdoc cref="IObjectManager.GameState"/>
        public string GameState { get; private set; }

        ///<inheritdoc cref="IObjectManager.IsTargetInLineOfSight"/>
        public bool IsTargetInLineOfSight { get; private set; }

        ///<inheritdoc cref="IObjectManager.IsWorldLoaded"/>
        public bool IsWorldLoaded { get; private set; }

        ///<inheritdoc cref="IObjectManager.LastTarget"/>
        public WowUnit LastTarget { get; private set; }

        ///<inheritdoc cref="IObjectManager.LastTargetGuid"/>
        public ulong LastTargetGuid { get; private set; }

        ///<inheritdoc cref="IObjectManager.MapId"/>
        public WowMapId MapId { get; private set; }

        ///<inheritdoc cref="IObjectManager.MeanGroupPosition"/>
        public Vector3 MeanGroupPosition { get; private set; }

        ///<inheritdoc cref="IObjectManager.ObjectCount"/>
        public int ObjectCount { get; set; }

        ///<inheritdoc cref="IObjectManager.Partyleader"/>
        public WowUnit Partyleader { get; private set; }

        ///<inheritdoc cref="IObjectManager.PartyleaderGuid"/>
        public ulong PartyleaderGuid { get; private set; }

        ///<inheritdoc cref="IObjectManager.PartymemberGuids"/>
        public IEnumerable<ulong> PartymemberGuids { get; private set; }

        ///<inheritdoc cref="IObjectManager.Partymembers"/>
        public IEnumerable<WowUnit> Partymembers { get; private set; }

        ///<inheritdoc cref="IObjectManager.PartyPetGuids"/>
        public IEnumerable<ulong> PartyPetGuids { get; private set; }

        ///<inheritdoc cref="IObjectManager.PartyPets"/>
        public IEnumerable<WowUnit> PartyPets { get; private set; }

        ///<inheritdoc cref="IObjectManager.Pet"/>
        public WowUnit Pet { get; private set; }

        ///<inheritdoc cref="IObjectManager.PetGuid"/>
        public ulong PetGuid { get; private set; }

        ///<inheritdoc cref="IObjectManager.Player"/>
        public WowPlayer Player { get; private set; }

        ///<inheritdoc cref="IObjectManager.PlayerBase"/>
        public IntPtr PlayerBase { get; private set; }

        ///<inheritdoc cref="IObjectManager.PlayerGuid"/>
        public ulong PlayerGuid { get; private set; }

        ///<inheritdoc cref="IObjectManager.Target"/>
        public WowUnit Target { get; private set; }

        ///<inheritdoc cref="IObjectManager.TargetGuid"/>
        public ulong TargetGuid { get; private set; }

        ///<inheritdoc cref="IObjectManager.Vehicle"/>
        public WowUnit Vehicle { get; private set; }

        ///<inheritdoc cref="IObjectManager.WowObjects"/>
        public IEnumerable<WowObject> WowObjects { get { lock (queryLock) { return wowObjects; } } }

        ///<inheritdoc cref="IObjectManager.ZoneId"/>
        public int ZoneId { get; private set; }

        ///<inheritdoc cref="IObjectManager.ZoneName"/>
        public string ZoneName { get; private set; }

        ///<inheritdoc cref="IObjectManager.ZoneSubName"/>
        public string ZoneSubName { get; private set; }

        private AmeisenBotConfig Config { get; }

        private bool PlayerGuidIsVehicle { get; set; }

        private TimegatedEvent PoiCacheEvent { get; }

        private TimegatedEvent RelationshipEvent { get; }

        private WowInterface WowInterface { get; }

        ///<inheritdoc cref="IObjectManager.GetAoeSpells(Vector3, bool, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<WowDynobject> GetAoeSpells(Vector3 position, bool onlyEnemy = true, float extends = 2.0f)
        {
            return WowInterface.ObjectManager.WowObjects.OfType<WowDynobject>()
                .Where(e => e.Position.GetDistance(position) < e.Radius + extends
                    && (!onlyEnemy || WowInterface.HookManager.WowGetUnitReaction(WowInterface.Player, WowInterface.ObjectManager.GetWowObjectByGuid<WowUnit>(e.Caster)) != WowUnitReaction.Friendly));
        }

        ///<inheritdoc cref="IObjectManager.GetClosestWowGameobjectByDisplayId(IEnumerable{int})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowGameobject GetClosestWowGameobjectByDisplayId(IEnumerable<int> displayIds)
        {
            lock (queryLock)
            {
                return wowObjects.OfType<WowGameobject>()
                    .Where(e => displayIds.Contains(e.DisplayId))
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();
            }
        }

        ///<inheritdoc cref="IObjectManager.GetClosestWowUnitByDisplayId(IEnumerable{int}, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowUnit GetClosestWowUnitByDisplayId(IEnumerable<int> displayIds, bool onlyQuestgiver = true)
        {
            lock (queryLock)
            {
                return wowObjects.OfType<WowUnit>()
                    .Where(e => (e.IsQuestgiver || !onlyQuestgiver) && displayIds.Contains(e.DisplayId))
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();
            }
        }

        ///<inheritdoc cref="IObjectManager.GetClosestWowUnitByNpcId(IEnumerable{int}, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WowUnit GetClosestWowUnitByNpcId(IEnumerable<int> npcIds, bool onlyQuestgiver = true)
        {
            lock (queryLock)
            {
                return wowObjects.OfType<WowUnit>()
                    .Where(e => (e.IsQuestgiver || !onlyQuestgiver) && npcIds.Contains(WowGuid.ToNpcId(e.Guid)))
                    .OrderBy(e => e.Position.GetDistance(WowInterface.Player.Position))
                    .FirstOrDefault();
            }
        }

        ///<inheritdoc cref="IObjectManager.GetEnemiesInCombatWithParty{T}(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesInCombatWithParty<T>(Vector3 position, float distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return GetNearEnemies<T>(position, distance)
                    .Where(e => e.IsInCombat && (e.IsTaggedByMe || e.TargetGuid == WowInterface.PlayerGuid));
            }
        }

        ///<inheritdoc cref="IObjectManager.GetEnemiesInPath{T}(IEnumerable{Vector3}, float)"/>
        public IEnumerable<T> GetEnemiesInPath<T>(IEnumerable<Vector3> path, float distance) where T : WowUnit
        {
            foreach (Vector3 pathPosition in path)
            {
                IEnumerable<T> nearEnemies = WowInterface.ObjectManager.GetNearEnemies<T>(pathPosition, distance);

                if (nearEnemies.Any())
                {
                    return nearEnemies;
                }
            }

            return Array.Empty<T>();
        }

        ///<inheritdoc cref="IObjectManager.GetEnemiesTargetingPartymembers{T}(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetEnemiesTargetingPartymembers<T>(Vector3 position, float distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return GetNearEnemies<T>(position, distance)
                    .Where(e => e.IsInCombat && (PartymemberGuids.Contains(e.TargetGuid) || PartyPetGuids.Contains(e.TargetGuid)));
            }
        }

        ///<inheritdoc cref="IObjectManager.MeanGroupPosition"/>
        public Vector3 GetMeanGroupPosition(bool includeSelf = false)
        {
            Vector3 meanGroupPosition = new();
            float count = 0;

            foreach (WowUnit unit in Partymembers)
            {
                if ((includeSelf || unit.Guid != PlayerGuid) && unit.Position.GetDistance(Player.Position) < 100.0f)
                {
                    meanGroupPosition += unit.Position;
                    ++count;
                }
            }

            return meanGroupPosition / count;
        }

        ///<inheritdoc cref="IObjectManager.GetNearEnemies{T}(Vector3, float)"/>
        public IEnumerable<T> GetNearEnemies<T>(Vector3 position, float distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>()
                    .Where(e => !e.IsDead
                         && !e.IsNotAttackable
                         && WowInterface.HookManager.WowGetUnitReaction(Player, e) != WowUnitReaction.Friendly
                         && WowInterface.HookManager.WowGetUnitReaction(Player, e) != WowUnitReaction.Neutral
                         && e.Position.GetDistance(position) < distance);
            }
        }

        ///<inheritdoc cref="IObjectManager.GetNearFriends{T}(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearFriends<T>(Vector3 position, float distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>()
                    .Where(e => !e.IsDead
                             && !e.IsNotAttackable
                             && WowInterface.HookManager.WowGetUnitReaction(Player, e) == WowUnitReaction.Friendly
                             && e.Position.GetDistance(position) < distance);
            }
        }

        ///<inheritdoc cref="IObjectManager.GetNearPartymembers{T}(Vector3, float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> GetNearPartymembers<T>(Vector3 position, float distance) where T : WowUnit
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>()
                    .Where(e => !e.IsDead
                             && !e.IsNotAttackable
                             && (PartymemberGuids.Contains(e.Guid) || PartyPetGuids.Contains(e.Guid))
                             && e.Position.GetDistance(position) < distance);
            }
        }

        ///<inheritdoc cref="IObjectManager.GetWowObjectByGuid{T}(ulong)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetWowObjectByGuid<T>(ulong guid) where T : WowObject
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>().FirstOrDefault(e => e.Guid == guid);
            }
        }

        ///<inheritdoc cref="IObjectManager.GetWowUnitByName{T}(string, StringComparison)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetWowUnitByName<T>(string name, StringComparison stringComparison = StringComparison.Ordinal) where T : WowUnit
        {
            lock (queryLock)
            {
                return wowObjects.OfType<T>().FirstOrDefault(e => e.Name.Equals(name, stringComparison));
            }
        }

        ///<inheritdoc cref="IObjectManager.RefreshIsWorldLoaded"/>
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

        ///<inheritdoc cref="IObjectManager.UpdateWowObjects"/>
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
                MapId = UpdateGlobalVar<WowMapId>(WowInterface.OffsetList.MapId);
                ZoneId = UpdateGlobalVar<int>(WowInterface.OffsetList.ZoneId);
                GameState = UpdateGlobalVarString(WowInterface.OffsetList.GameState);

                if (WowInterface.XMemory.Read(WowInterface.OffsetList.CameraPointer, out IntPtr cameraPointer)
                    && WowInterface.XMemory.Read(IntPtr.Add(cameraPointer, (int)WowInterface.OffsetList.CameraOffset), out cameraPointer))
                {
                    Camera = UpdateGlobalVar<RawCameraInfo>(cameraPointer);
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

                int c = 0;
                Array.Clear(wowObjectPointers, 0, MAX_OBJECT_COUNT);

                for (; (int)activeObjectBaseAddress > 0 && c < MAX_OBJECT_COUNT; ++c)
                {
                    wowObjectPointers[c] = activeObjectBaseAddress;
                    WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)WowInterface.OffsetList.NextObject), out activeObjectBaseAddress);
                }

                ObjectCount = c;
                Parallel.For(0, c, x => ProcessObject(x));

                if (PlayerGuidIsVehicle)
                {
                    // get the object with last known "good" guid
                    WowPlayer lastKnownPlayer = wowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.Guid == Player.Guid);

                    if (lastKnownPlayer != null)
                    {
                        Player = lastKnownPlayer;
                    }
                }

                // read the party/raid leaders guid and if there is one, the group too
                PartyleaderGuid = ReadLeaderGuid();

                if (PartyleaderGuid > 0)
                {
                    PartymemberGuids = ReadPartymemberGuids();
                    Partymembers = wowObjects.OfType<WowUnit>().Where(e => PartymemberGuids.Contains(e.Guid));

                    MeanGroupPosition = GetMeanGroupPosition();

                    PartyPetGuids = PartyPets.Select(e => e.Guid);
                    PartyPets = wowObjects.OfType<WowUnit>().Where(e => PartymemberGuids.Contains(e.SummonedByGuid));
                }
            }

            if (Config.CachePointsOfInterest && PoiCacheEvent.Run())
            {
                CachePois();
            }

            // if (RelationshipEvent.Run())
            // {
            //     CheckGroupRelationships();
            // }

            OnObjectUpdateComplete?.Invoke(wowObjects);
        }

        /// <summary>
        /// Saves all interesting points in the db, for example herbs, ores and mailboxes.
        /// </summary>
        private void CachePois()
        {
            IEnumerable<WowGameobject> wowGameobjects = wowObjects.OfType<WowGameobject>();
            IEnumerable<WowUnit> wowUnits = wowObjects.OfType<WowUnit>();

            // Remember Ore/Herb positions for farming
            foreach (WowGameobject gameobject in wowGameobjects.Where(e => Enum.IsDefined(typeof(WowOreId), e.DisplayId)))
            {
                WowInterface.Db.CacheOre(MapId, (WowOreId)gameobject.DisplayId, gameobject.Position);
            }

            foreach (WowGameobject gameobject in wowGameobjects.Where(e => Enum.IsDefined(typeof(WowHerbId), e.DisplayId)))
            {
                WowInterface.Db.CacheHerb(MapId, (WowHerbId)gameobject.DisplayId, gameobject.Position);
            }

            // Remember Mailboxes
            foreach (WowGameobject gameobject in wowGameobjects.Where(e => e.GameobjectType == WowGameobjectType.Mailbox))
            {
                WowInterface.Db.CachePoi(MapId, PoiType.Mailbox, gameobject.Position);
            }

            // Remember Auctioneers
            foreach (WowUnit unit in wowUnits.Where(e => e.IsAuctioneer))
            {
                WowInterface.Db.CachePoi(MapId, PoiType.Auctioneer, unit.Position);
            }

            // Remember Fishingspots and places where people fished at
            foreach (WowGameobject gameobject in wowGameobjects.Where(e => e.GameobjectType == WowGameobjectType.FishingHole || e.GameobjectType == WowGameobjectType.FishingBobber))
            {
                WowUnit originUnit = WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == gameobject.CreatedBy);

                // dont cache positions too close to eachother
                if (originUnit != null
                    && !WowInterface.Db.TryGetPointsOfInterest(MapId, PoiType.FishingSpot, originUnit.Position, 5.0f, out IEnumerable<Vector3> pois))
                {
                    WowInterface.Db.CachePoi(MapId, PoiType.FishingSpot, originUnit.Position);
                }
            }

            // Remember Vendors
            foreach (WowUnit unit in wowUnits.Where(e => e.IsVendor))
            {
                WowInterface.Db.CachePoi(MapId, PoiType.Vendor, unit.Position);
            }

            // Remember Repair Vendors
            foreach (WowUnit unit in wowUnits.Where(e => e.IsRepairVendor))
            {
                WowInterface.Db.CachePoi(MapId, PoiType.Repair, unit.Position);
            }
        }

        private void CheckGroupRelationships()
        {
            IEnumerable<WowPlayer> wowPlayers = wowObjects.OfType<WowPlayer>();

            foreach (WowPlayer player in wowPlayers)
            {
                if (player.Guid != PlayerGuid)
                {
                    // check whether its a new player old someone we know
                    if (!WowInterface.Db.IsPlayerKnown(player))
                    {
                        WowInterface.Db.AddPlayerRelationship(player,
                            // is the player in my friend list
                            Config.Friends.Contains(player.Name, StringComparison.OrdinalIgnoreCase) ?
                                RelationshipLevel.Friend :
                                // is the player in my group
                                PartymemberGuids.Contains(player.Guid) ?
                                    RelationshipLevel.Positive :
                                    // is the player hostile
                                    WowInterface.HookManager.WowGetUnitReaction(Player, player) == WowUnitReaction.Hostile ?
                                        RelationshipLevel.Negative :
                                        RelationshipLevel.Neutral);
                    }
                    else
                    {
                        WowInterface.Db.UpdatePlayerRelationship(player);
                    }
                }
            }
        }

        /// <summary>
        /// Process the pushed game info that we receive from the EndScene hook.
        /// </summary>
        /// <param name="gameInfo"></param>
        private void HookManagerOnGameInfoPush(GameInfo gameInfo)
        {
            if (Player != null)
            {
                Player.IsOutdoors = gameInfo.isOutdoors;
            }

            IsTargetInLineOfSight = gameInfo.isTargetInLineOfSight;
        }

        /// <summary>
        /// Process a wow object pointer into a full object. Object will be placed
        /// in "wowObjects", pointers will be taken from "wowObjectPointers".
        /// </summary>
        /// <param name="i">Index of the object</param>
        private void ProcessObject(int i)
        {
            IntPtr ptr = wowObjectPointers[i];

            if (ptr != IntPtr.Zero
                && WowInterface.XMemory.Read(IntPtr.Add(ptr, (int)WowInterface.OffsetList.WowObjectType), out WowObjectType type)
                && WowInterface.XMemory.Read(IntPtr.Add(ptr, (int)WowInterface.OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress))
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

                obj.Update(WowInterface);

                if (type == WowObjectType.Unit || type == WowObjectType.Player)
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

                wowObjects[i] = obj;
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
            List<ulong> partymemberGuids = new();

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
                ConcurrentBag<ulong> guids = new();

                foreach (IntPtr raidPointer in raidPointers)
                {
                    if (WowInterface.XMemory.Read(raidPointer, out ulong guid) && guid != 0)
                    {
                        guids.Add(guid);
                    }
                }

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