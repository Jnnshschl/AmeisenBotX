using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Hook.Structs;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Wow335a.Objects
{
    public class ObjectManager : IObjectProvider
    {
        private const int MAX_OBJECT_COUNT = 4096;

        private readonly object queryLock = new();

        private readonly IntPtr[] wowObjectPointers;
        private readonly IWowObject[] wowObjects;

        public ObjectManager(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            MemoryApi = memoryApi;
            OffsetList = offsetList;

            wowObjectPointers = new IntPtr[MAX_OBJECT_COUNT];
            wowObjects = new IWowObject[MAX_OBJECT_COUNT];

            PartymemberGuids = new List<ulong>();
            PartyPetGuids = new List<ulong>();
            Partymembers = new List<WowUnit335a>();
            PartyPets = new List<WowUnit335a>();
        }

        ///<inheritdoc cref="IObjectProvider.OnObjectUpdateComplete"/>
        public event Action<IEnumerable<IWowObject>> OnObjectUpdateComplete;

        ///<inheritdoc cref="IObjectProvider.Camera"/>
        public RawCameraInfo Camera { get; private set; }

        ///<inheritdoc cref="IObjectProvider.CenterPartyPosition"/>
        public Vector3 CenterPartyPosition { get; private set; }

        ///<inheritdoc cref="IObjectProvider.GameState"/>
        public string GameState { get; private set; }

        ///<inheritdoc cref="IObjectProvider.IsTargetInLineOfSight"/>
        public bool IsTargetInLineOfSight { get; private set; }

        ///<inheritdoc cref="IObjectProvider.IsWorldLoaded"/>
        public bool IsWorldLoaded { get; private set; }

        ///<inheritdoc cref="IObjectProvider.LastTarget"/>
        public IWowUnit LastTarget { get; private set; }

        ///<inheritdoc cref="IObjectProvider.LastTargetGuid"/>
        public ulong LastTargetGuid { get; private set; }

        ///<inheritdoc cref="IObjectProvider.MapId"/>
        public WowMapId MapId { get; private set; }

        ///<inheritdoc cref="IObjectProvider.ObjectCount"/>
        public int ObjectCount { get; set; }

        ///<inheritdoc cref="IObjectProvider.Partyleader"/>
        public IWowUnit Partyleader { get; private set; }

        ///<inheritdoc cref="IObjectProvider.PartyleaderGuid"/>
        public ulong PartyleaderGuid { get; private set; }

        ///<inheritdoc cref="IObjectProvider.PartymemberGuids"/>
        public IEnumerable<ulong> PartymemberGuids { get; private set; }

        ///<inheritdoc cref="IObjectProvider.Partymembers"/>
        public IEnumerable<IWowUnit> Partymembers { get; private set; }

        ///<inheritdoc cref="IObjectProvider.PartyPetGuids"/>
        public IEnumerable<ulong> PartyPetGuids { get; private set; }

        ///<inheritdoc cref="IObjectProvider.PartyPets"/>
        public IEnumerable<IWowUnit> PartyPets { get; private set; }

        ///<inheritdoc cref="IObjectProvider.Pet"/>
        public IWowUnit Pet { get; private set; }

        ///<inheritdoc cref="IObjectProvider.PetGuid"/>
        public ulong PetGuid { get; private set; }

        ///<inheritdoc cref="IObjectProvider.Player"/>
        public IWowPlayer Player { get; private set; }

        ///<inheritdoc cref="IObjectProvider.PlayerBase"/>
        public IntPtr PlayerBase { get; private set; }

        ///<inheritdoc cref="IObjectProvider.PlayerGuid"/>
        public ulong PlayerGuid { get; private set; }

        ///<inheritdoc cref="IObjectProvider.Target"/>
        public IWowUnit Target { get; private set; }

        ///<inheritdoc cref="IObjectProvider.TargetGuid"/>
        public ulong TargetGuid { get; private set; }

        ///<inheritdoc cref="IObjectProvider.Vehicle"/>
        public IWowUnit Vehicle { get; private set; }

        ///<inheritdoc cref="IObjectProvider.WowObjects"/>
        public IEnumerable<IWowObject> WowObjects { get { lock (queryLock) { return wowObjects; } } }

        ///<inheritdoc cref="IObjectProvider.ZoneId"/>
        public int ZoneId { get; private set; }

        ///<inheritdoc cref="IObjectProvider.ZoneName"/>
        public string ZoneName { get; private set; }

        ///<inheritdoc cref="IObjectProvider.ZoneSubName"/>
        public string ZoneSubName { get; private set; }

        private IMemoryApi MemoryApi { get; }

        private IOffsetList OffsetList { get; }

        private bool PlayerGuidIsVehicle { get; set; }

        ///<inheritdoc cref="IObjectProvider.RefreshIsWorldLoaded"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefreshIsWorldLoaded()
        {
            if (MemoryApi.Read(OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                IsWorldLoaded = isWorldLoaded == 1;
                return IsWorldLoaded;
            }

            return false;
        }

        ///<inheritdoc cref="IObjectProvider.UpdateWowObjects"/>
        public void UpdateWowObjects()
        {
            IsWorldLoaded = UpdateGlobalVar<int>(OffsetList.IsWorldLoaded) == 1;

            if (!IsWorldLoaded) { return; }

            lock (queryLock)
            {
                PlayerGuid = UpdateGlobalVar<ulong>(OffsetList.PlayerGuid);
                TargetGuid = UpdateGlobalVar<ulong>(OffsetList.TargetGuid);
                LastTargetGuid = UpdateGlobalVar<ulong>(OffsetList.LastTargetGuid);
                PetGuid = UpdateGlobalVar<ulong>(OffsetList.PetGuid);
                PlayerBase = UpdateGlobalVar<IntPtr>(OffsetList.PlayerBase);
                MapId = UpdateGlobalVar<WowMapId>(OffsetList.MapId);
                ZoneId = UpdateGlobalVar<int>(OffsetList.ZoneId);
                GameState = UpdateGlobalVarString(OffsetList.GameState);

                if (MemoryApi.Read(OffsetList.CameraPointer, out IntPtr cameraPointer)
                    && MemoryApi.Read(IntPtr.Add(cameraPointer, (int)OffsetList.CameraOffset), out cameraPointer))
                {
                    Camera = UpdateGlobalVar<RawCameraInfo>(cameraPointer);
                }

                if (MemoryApi.Read(OffsetList.ZoneText, out IntPtr zoneNamePointer))
                {
                    ZoneName = UpdateGlobalVarString(zoneNamePointer);
                }

                if (MemoryApi.Read(OffsetList.ZoneSubText, out IntPtr zoneSubNamePointer))
                {
                    ZoneSubName = UpdateGlobalVarString(zoneSubNamePointer);
                }

                if (TargetGuid == 0) { Target = null; }
                if (PetGuid == 0) { Pet = null; }
                if (LastTargetGuid == 0) { LastTarget = null; }
                if (PartyleaderGuid == 0) { Partyleader = null; }

                MemoryApi.Read(OffsetList.ClientConnection, out IntPtr clientConnection);
                MemoryApi.Read(IntPtr.Add(clientConnection, (int)OffsetList.CurrentObjectManager), out IntPtr currentObjectManager);

                // read the first object
                MemoryApi.Read(IntPtr.Add(currentObjectManager, (int)OffsetList.FirstObject), out IntPtr activeObjectBaseAddress);

                int c = 0;
                Array.Clear(wowObjectPointers, 0, MAX_OBJECT_COUNT);

                for (; (int)activeObjectBaseAddress > 0 && c < MAX_OBJECT_COUNT; ++c)
                {
                    wowObjectPointers[c] = activeObjectBaseAddress;
                    MemoryApi.Read(IntPtr.Add(activeObjectBaseAddress, (int)OffsetList.NextObject), out activeObjectBaseAddress);
                }

                ObjectCount = c;
                Parallel.For(0, c, x => ProcessObject(x));

                if (PlayerGuidIsVehicle)
                {
                    // get the object with last known "good" guid
                    WowPlayer335a lastKnownPlayer = wowObjects.OfType<WowPlayer335a>().FirstOrDefault(e => e.Guid == Player.Guid);

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
                    Partymembers = wowObjects.OfType<WowUnit335a>().Where(e => PartymemberGuids.Contains(e.Guid));

                    PartyPetGuids = PartyPets.Select(e => e.Guid);
                    PartyPets = wowObjects.OfType<WowUnit335a>().Where(e => PartymemberGuids.Contains(e.SummonedByGuid));
                }
            }

            OnObjectUpdateComplete?.Invoke(wowObjects);
        }

        /// <summary>
        /// Process the pushed game info that we receive from the EndScene hook.
        /// </summary>
        /// <param name="gameInfo"></param>
        internal void HookManagerOnGameInfoPush(GameInfo gameInfo)
        {
            if (Player != null)
            {
                ((WowPlayer335a)Player).IsOutdoors = gameInfo.isOutdoors;
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
                && MemoryApi.Read(IntPtr.Add(ptr, (int)OffsetList.WowObjectType), out WowObjectType type)
                && MemoryApi.Read(IntPtr.Add(ptr, (int)OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress))
            {
                wowObjects[i] = type switch
                {
                    WowObjectType.Container => new WowContainer335a(ptr, descriptorAddress),
                    WowObjectType.Corpse => new WowCorpse335a(ptr, descriptorAddress),
                    WowObjectType.Item => new WowItem335a(ptr, descriptorAddress),
                    WowObjectType.Dynobject => new WowDynobject335a(ptr, descriptorAddress),
                    WowObjectType.Gameobject => new WowGameobject335a(ptr, descriptorAddress),
                    WowObjectType.Player => new WowPlayer335a(ptr, descriptorAddress),
                    WowObjectType.Unit => new WowUnit335a(ptr, descriptorAddress),
                    _ => new WowObject335a(ptr, descriptorAddress),
                };

                wowObjects[i].Update(MemoryApi, OffsetList);

                if (type is WowObjectType.Unit or WowObjectType.Player)
                {
                    if (wowObjects[i].Guid == PlayerGuid)
                    {
                        PlayerGuidIsVehicle = wowObjects[i].GetType() != typeof(WowPlayer335a);

                        if (!PlayerGuidIsVehicle)
                        {
                            Player = (WowPlayer335a)wowObjects[i];
                            Vehicle = null;
                        }
                        else
                        {
                            // player stays the old object
                            Vehicle = (WowUnit335a)wowObjects[i];
                        }
                    }

                    if (wowObjects[i].Guid == TargetGuid) { Target = (WowUnit335a)wowObjects[i]; }
                    if (wowObjects[i].Guid == LastTargetGuid) { LastTarget = (WowUnit335a)wowObjects[i]; }
                    if (wowObjects[i].Guid == PartyleaderGuid) { Partyleader = (WowUnit335a)wowObjects[i]; }
                    if (wowObjects[i].Guid == PetGuid) { Pet = (WowUnit335a)wowObjects[i]; }
                }
            }
        }

        private ulong ReadLeaderGuid()
        {
            if (MemoryApi.Read(OffsetList.RaidLeader, out ulong partyleaderGuid))
            {
                if (partyleaderGuid == 0
                    && MemoryApi.Read(OffsetList.PartyLeader, out partyleaderGuid))
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

            if (MemoryApi.Read(OffsetList.PartyLeader, out ulong partyLeader)
                && partyLeader != 0
                && MemoryApi.Read(OffsetList.PartyPlayerGuids, out RawPartyGuids partyMembers))
            {
                partymemberGuids.AddRange(partyMembers.AsArray());
            }

            if (MemoryApi.Read(OffsetList.RaidLeader, out ulong raidLeader)
                && raidLeader != 0
                && MemoryApi.Read(OffsetList.RaidGroupStart, out RawRaidStruct raidStruct))
            {
                IEnumerable<IntPtr> raidPointers = raidStruct.GetPointers();
                ConcurrentBag<ulong> guids = new();

                foreach (IntPtr raidPointer in raidPointers)
                {
                    if (MemoryApi.Read(raidPointer, out ulong guid) && guid != 0)
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
            return MemoryApi.Read(address, out T v) ? v : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string UpdateGlobalVarString(IntPtr address, int maxLenght = 128)
        {
            return MemoryApi.ReadString(address, Encoding.UTF8, out string v, maxLenght) ? v : string.Empty;
        }
    }
}