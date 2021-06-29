using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Core.Data.Objects;
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
        private readonly WowObject[] wowObjects;

        public ObjectManager(XMemory xMemory, IOffsetList offsetList)
        {
            XMemory = xMemory;
            OffsetList = offsetList;

            wowObjectPointers = new IntPtr[MAX_OBJECT_COUNT];
            wowObjects = new WowObject[MAX_OBJECT_COUNT];

            PartymemberGuids = new List<ulong>();
            PartyPetGuids = new List<ulong>();
            Partymembers = new List<WowUnit>();
            PartyPets = new List<WowUnit>();
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

        private bool PlayerGuidIsVehicle { get; set; }

        private XMemory XMemory { get; }

        private IOffsetList OffsetList { get; }

        ///<inheritdoc cref="IObjectManager.RefreshIsWorldLoaded"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefreshIsWorldLoaded()
        {
            if (XMemory.Read(OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                IsWorldLoaded = isWorldLoaded == 1;
                return IsWorldLoaded;
            }

            return false;
        }

        ///<inheritdoc cref="IObjectManager.UpdateWowObjects"/>
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

                if (XMemory.Read(OffsetList.CameraPointer, out IntPtr cameraPointer)
                    && XMemory.Read(IntPtr.Add(cameraPointer, (int)OffsetList.CameraOffset), out cameraPointer))
                {
                    Camera = UpdateGlobalVar<RawCameraInfo>(cameraPointer);
                }

                if (XMemory.Read(OffsetList.ZoneText, out IntPtr zoneNamePointer))
                {
                    ZoneName = UpdateGlobalVarString(zoneNamePointer);
                }

                if (XMemory.Read(OffsetList.ZoneSubText, out IntPtr zoneSubNamePointer))
                {
                    ZoneSubName = UpdateGlobalVarString(zoneSubNamePointer);
                }

                if (TargetGuid == 0) { Target = null; }
                if (PetGuid == 0) { Pet = null; }
                if (LastTargetGuid == 0) { LastTarget = null; }
                if (PartyleaderGuid == 0) { Partyleader = null; }

                XMemory.Read(OffsetList.ClientConnection, out IntPtr clientConnection);
                XMemory.Read(IntPtr.Add(clientConnection, (int)OffsetList.CurrentObjectManager), out IntPtr currentObjectManager);

                // read the first object
                XMemory.Read(IntPtr.Add(currentObjectManager, (int)OffsetList.FirstObject), out IntPtr activeObjectBaseAddress);

                int c = 0;
                Array.Clear(wowObjectPointers, 0, MAX_OBJECT_COUNT);

                for (; (int)activeObjectBaseAddress > 0 && c < MAX_OBJECT_COUNT; ++c)
                {
                    wowObjectPointers[c] = activeObjectBaseAddress;
                    XMemory.Read(IntPtr.Add(activeObjectBaseAddress, (int)OffsetList.NextObject), out activeObjectBaseAddress);
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

                    PartyPetGuids = PartyPets.Select(e => e.Guid);
                    PartyPets = wowObjects.OfType<WowUnit>().Where(e => PartymemberGuids.Contains(e.SummonedByGuid));
                }
            }

            OnObjectUpdateComplete?.Invoke(wowObjects);
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
                && XMemory.Read(IntPtr.Add(ptr, (int)OffsetList.WowObjectType), out WowObjectType type)
                && XMemory.Read(IntPtr.Add(ptr, (int)OffsetList.WowObjectDescriptor), out IntPtr descriptorAddress))
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

                obj.Update(XMemory, OffsetList);

                if (type == WowObjectType.Unit || type == WowObjectType.Player)
                {
                    if (obj.Guid == PlayerGuid)
                    {
                        PlayerGuidIsVehicle = obj.GetType() != typeof(WowPlayer);

                        if (!PlayerGuidIsVehicle)
                        {
                            if (XMemory.Read(OffsetList.ComboPoints, out byte comboPoints))
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
            if (XMemory.Read(OffsetList.RaidLeader, out ulong partyleaderGuid))
            {
                if (partyleaderGuid == 0
                    && XMemory.Read(OffsetList.PartyLeader, out partyleaderGuid))
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

            if (XMemory.Read(OffsetList.PartyLeader, out ulong partyLeader)
                && partyLeader != 0
                && XMemory.Read(OffsetList.PartyPlayerGuids, out RawPartyGuids partyMembers))
            {
                partymemberGuids.AddRange(partyMembers.AsArray());
            }

            if (XMemory.Read(OffsetList.RaidLeader, out ulong raidLeader)
                && raidLeader != 0
                && XMemory.Read(OffsetList.RaidGroupStart, out RawRaidStruct raidStruct))
            {
                IEnumerable<IntPtr> raidPointers = raidStruct.GetPointers();
                ConcurrentBag<ulong> guids = new();

                foreach (IntPtr raidPointer in raidPointers)
                {
                    if (XMemory.Read(raidPointer, out ulong guid) && guid != 0)
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
            return XMemory.Read(address, out T v) ? v : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string UpdateGlobalVarString(IntPtr address, int maxLenght = 128)
        {
            return XMemory.ReadString(address, Encoding.UTF8, out string v, maxLenght) ? v : string.Empty;
        }
    }
}
