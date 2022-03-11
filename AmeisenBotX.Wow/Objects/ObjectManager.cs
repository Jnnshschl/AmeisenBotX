using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Hook.Structs;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Raw;
using AmeisenBotX.Wow.Offsets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Wow.Objects
{
    public abstract class ObjectManager<TObject, TUnit, TPlayer, TGameobject, TDynobject, TItem, TCorpse, TContainer> : IObjectProvider
        where TObject : IWowObject, new()
        where TUnit : IWowUnit, new()
        where TPlayer : IWowPlayer, new()
        where TGameobject : IWowGameobject, new()
        where TDynobject : IWowDynobject, new()
        where TItem : IWowItem, new()
        where TCorpse : IWowCorpse, new()
        where TContainer : IWowContainer, new()

    {
        protected const int MAX_OBJECT_COUNT = 4096;

        protected readonly object queryLock = new();

        protected readonly IntPtr[] wowObjectPointers;
        protected readonly IWowObject[] wowObjects;

        public ObjectManager(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            MemoryApi = memoryApi;
            OffsetList = offsetList;

            wowObjectPointers = new IntPtr[MAX_OBJECT_COUNT];
            wowObjects = new IWowObject[MAX_OBJECT_COUNT];

            PartymemberGuids = new List<ulong>();
            PartyPetGuids = new List<ulong>();
            Partymembers = new List<IWowUnit>();
            PartyPets = new List<IWowUnit>();
        }

        ///<inheritdoc cref="IObjectProvider.OnObjectUpdateComplete"/>
        public event Action<IEnumerable<IWowObject>> OnObjectUpdateComplete;

        ///<inheritdoc cref="IObjectProvider.Camera"/>
        public RawCameraInfo Camera { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.CenterPartyPosition"/>
        public Vector3 CenterPartyPosition { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.GameState"/>
        public string GameState { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.IsTargetInLineOfSight"/>
        public bool IsTargetInLineOfSight { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.IsWorldLoaded"/>
        public bool IsWorldLoaded { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.LastTarget"/>
        public IWowUnit LastTarget { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.LastTargetGuid"/>
        public ulong LastTargetGuid { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.MapId"/>
        public WowMapId MapId { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.ObjectCount"/>
        public int ObjectCount { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.Partyleader"/>
        public IWowUnit Partyleader { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.PartyleaderGuid"/>
        public ulong PartyleaderGuid { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.PartymemberGuids"/>
        public IEnumerable<ulong> PartymemberGuids { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.Partymembers"/>
        public IEnumerable<IWowUnit> Partymembers { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.PartyPetGuids"/>
        public IEnumerable<ulong> PartyPetGuids { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.PartyPets"/>
        public IEnumerable<IWowUnit> PartyPets { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.Pet"/>
        public IWowUnit Pet { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.PetGuid"/>
        public ulong PetGuid { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.Player"/>
        public IWowPlayer Player { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.PlayerBase"/>
        public IntPtr PlayerBase { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.PlayerGuid"/>
        public ulong PlayerGuid { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.Target"/>
        public IWowUnit Target { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.TargetGuid"/>
        public ulong TargetGuid { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.Vehicle"/>
        public IWowUnit Vehicle { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.WowObjects"/>
        public IEnumerable<IWowObject> WowObjects { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.ZoneId"/>
        public int ZoneId { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.ZoneName"/>
        public string ZoneName { get; protected set; }

        ///<inheritdoc cref="IObjectProvider.ZoneSubName"/>
        public string ZoneSubName { get; protected set; }

        protected IMemoryApi MemoryApi { get; }

        protected IOffsetList OffsetList { get; }

        protected bool PlayerGuidIsVehicle { get; set; }

        /// <summary>
        /// Process the pushed game info that we receive from the EndScene hook.
        /// </summary>
        /// <param name="gameInfo"></param>
        public void HookManagerOnGameInfoPush(GameInfo gameInfo)
        {
            if (Player != null)
            {
                ((TPlayer)Player).IsOutdoors = gameInfo.isOutdoors;
            }

            IsTargetInLineOfSight = TargetGuid == 0 || TargetGuid == PlayerGuid || (gameInfo.losCheckResult & 0xFF) == 0;
            // AmeisenLogger.I.Log("GameInfo", $"IsTargetInLineOfSight: {IsTargetInLineOfSight}");
        }

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
            lock (queryLock)
            {
                IsWorldLoaded = UpdateGlobalVar<int>(OffsetList.IsWorldLoaded) == 1;

                if (!IsWorldLoaded) { return; }

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

                // if (MemoryApi.Read(OffsetList.ZoneText, out IntPtr zoneNamePointer)) { ZoneName =
                // UpdateGlobalVarString(zoneNamePointer); }

                // if (MemoryApi.Read(OffsetList.ZoneSubText, out IntPtr zoneSubNamePointer)) {
                // ZoneSubName = UpdateGlobalVarString(zoneSubNamePointer); }

                if (TargetGuid == 0) { Target = null; }
                if (PetGuid == 0) { Pet = null; }
                if (LastTargetGuid == 0) { LastTarget = null; }
                if (PartyleaderGuid == 0) { Partyleader = null; }

                MemoryApi.Read(OffsetList.ClientConnection, out IntPtr clientConnection);
                MemoryApi.Read(IntPtr.Add(clientConnection, (int)OffsetList.CurrentObjectManager), out IntPtr currentObjectManager);
                MemoryApi.Read(IntPtr.Add(currentObjectManager, (int)OffsetList.FirstObject), out IntPtr activeObjectBaseAddress);

                int c = 0;
                Array.Clear(wowObjects, 0, MAX_OBJECT_COUNT);
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
                    IWowPlayer lastKnownPlayer = wowObjects.OfType<IWowPlayer>().FirstOrDefault(e => e.Guid == Player.Guid);

                    if (lastKnownPlayer != null)
                    {
                        Player = lastKnownPlayer;
                    }
                }

                // read the party/raid leaders guid and if there is one, the group too
                ReadParty();

                WowObjects = wowObjects[0..ObjectCount];
                OnObjectUpdateComplete?.Invoke(WowObjects);
            }
        }

        protected abstract void ReadParty();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T UpdateGlobalVar<T>(IntPtr address) where T : unmanaged
        {
            return address != IntPtr.Zero && MemoryApi.Read(address, out T v) ? v : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string UpdateGlobalVarString(IntPtr address, int maxLenght = 128)
        {
            return address != IntPtr.Zero && MemoryApi.ReadString(address, Encoding.UTF8, out string v, maxLenght) ? v : string.Empty;
        }

        /// <summary>
        /// Process a wow object pointer into a full object. Object will be placed in "wowObjects",
        /// pointers will be taken from "wowObjectPointers".
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
                    WowObjectType.Container => new TContainer(),
                    WowObjectType.Corpse => new TCorpse(),
                    WowObjectType.Item => new TItem(),
                    WowObjectType.DynamicObject => new TDynobject(),
                    WowObjectType.GameObject => new TGameobject(),
                    WowObjectType.Player => new TPlayer(),
                    WowObjectType.Unit => new TUnit(),
                    _ => new TObject()
                };

                if (wowObjects[i] != null)
                {
                    wowObjects[i].Init(MemoryApi, OffsetList, ptr, descriptorAddress);

                    if (type is WowObjectType.Unit or WowObjectType.Player)
                    {
                        if (wowObjects[i].Guid == PlayerGuid)
                        {
                            PlayerGuidIsVehicle = wowObjects[i] is not TPlayer;

                            if (!PlayerGuidIsVehicle)
                            {
                                Player = (TPlayer)wowObjects[i];
                                Vehicle = null;
                            }
                            else
                            {
                                // player stays the old object
                                Vehicle = (TUnit)wowObjects[i];
                            }
                        }

                        if (wowObjects[i].Guid == TargetGuid) { Target = (TUnit)wowObjects[i]; }
                        if (wowObjects[i].Guid == LastTargetGuid) { LastTarget = (TUnit)wowObjects[i]; }
                        if (wowObjects[i].Guid == PartyleaderGuid) { Partyleader = (TUnit)wowObjects[i]; }
                        if (wowObjects[i].Guid == PetGuid) { Pet = (TUnit)wowObjects[i]; }
                    }
                }
            }
        }
    }
}