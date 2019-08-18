using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.OffsetLists;
using AmeisenBotX.Memory;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Data
{
    public class ObjectManager
    {
        private readonly object QueryLock = new object();

        private XMemory XMemory { get; }
        private CacheManager CacheManager { get; }
        private IOffsetList OffsetList { get; }

        private List<WowObject> wowObjects;

        public List<WowObject> WowObjects
        {
            get { lock (QueryLock) { return wowObjects; } }
            set { lock (QueryLock) { wowObjects = value; } }
        }

        public ulong PlayerGuid { get; private set; }
        public ulong TargetGuid { get; private set; }
        public ulong LastTargetGuid { get; private set; }
        public ulong PetGuid { get; private set; }
        public ulong PartyleaderGuid { get; private set; }
        public List<ulong> PartymemberGuids { get; private set; }

        public IntPtr PlayerBase { get; private set; }

        public bool IsWorldLoaded { get; private set; }

        public int MapId { get; private set; }
        public int ZoneId { get; private set; }

        public WowPlayer Player { get; private set; }

        public delegate void ObjectUpdateComplete(List<WowObject> wowObjects);
        public event ObjectUpdateComplete OnObjectUpdateComplete;

        public ObjectManager(XMemory xMemory, IOffsetList offsetList, CacheManager cacheManager)
        {
            IsWorldLoaded = true;

            WowObjects = new List<WowObject>();
            XMemory = xMemory;
            OffsetList = offsetList;
            CacheManager = cacheManager;

            EnableClickToMove();
        }

        private void EnableClickToMove()
        {
            if (XMemory.Read(OffsetList.ClickToMovePointer, out IntPtr ctmPointer)
                && XMemory.Read(IntPtr.Add(ctmPointer, OffsetList.ClickToMoveEnabled.ToInt32()), out int ctmEnabled))
            {
                if (ctmEnabled != 1) XMemory.Write(IntPtr.Add(ctmPointer, OffsetList.ClickToMoveEnabled.ToInt32()), 1);
            }
        }

        public bool RefreshIsWorldLoaded()
        {
            if (XMemory.Read(OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                IsWorldLoaded = isWorldLoaded == 1;
                return IsWorldLoaded;
            }
            return false;
        }

        public void UpdateWowObjects()
        {
            if (!XMemory.Read(OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                IsWorldLoaded = isWorldLoaded == 1;
                return;
            }

            if (XMemory.Read(OffsetList.PlayerGuid, out ulong playerGuid)) PlayerGuid = playerGuid;
            if (XMemory.Read(OffsetList.TargetGuid, out ulong targetGuid)) TargetGuid = targetGuid;
            if (XMemory.Read(OffsetList.TargetGuid, out ulong lastTargetGuid)) LastTargetGuid = lastTargetGuid;
            if (XMemory.Read(OffsetList.PetGuid, out ulong petGuid)) PetGuid = petGuid;
            if (XMemory.Read(OffsetList.TargetGuid, out IntPtr playerbase)) PlayerBase = playerbase;

            WowObjects = new List<WowObject>();
            XMemory.Read(OffsetList.ClientConnection, out IntPtr clientConnection);
            XMemory.Read(IntPtr.Add(clientConnection, OffsetList.CurrentObjectManager.ToInt32()), out IntPtr currentObjectManager);

            XMemory.Read(IntPtr.Add(currentObjectManager, OffsetList.FirstObject.ToInt32()), out IntPtr activeObject);
            XMemory.Read(IntPtr.Add(activeObject, OffsetList.WowObjectType.ToInt32()), out int objectType);

            while (isWorldLoaded == 1 && (objectType <= 7 && objectType > 0))
            {
                WowObjectType wowObjectType = (WowObjectType)objectType;
                switch (wowObjectType)
                {
                    case WowObjectType.Unit: WowObjects.Add(ReadWowUnit(activeObject, wowObjectType)); break;
                    case WowObjectType.Player: WowObjects.Add(ReadWowPlayer(activeObject, wowObjectType)); break;

                    default: WowObjects.Add(ReadWowObject(activeObject, wowObjectType)); break;
                }

                XMemory.Read(IntPtr.Add(activeObject, OffsetList.NextObject.ToInt32()), out activeObject);
                XMemory.Read(IntPtr.Add(activeObject, OffsetList.WowObjectType.ToInt32()), out objectType);
            }

            PartyleaderGuid = ReadPartyLeaderGuid();
            PartymemberGuids = ReadPartymemberGuids();

            if (XMemory.Read(OffsetList.MapId, out int mapId)) MapId = mapId;
            if (XMemory.Read(OffsetList.ZoneId, out int zoneId)) ZoneId = zoneId;

            OnObjectUpdateComplete?.Invoke(WowObjects);
        }

        public WowObject ReadWowObject(IntPtr activeObject, WowObjectType wowObjectType = WowObjectType.None)
        {
            if (XMemory.Read(IntPtr.Add(activeObject, OffsetList.WowObjectDescriptor.ToInt32()), out IntPtr descriptorAddress)
                && XMemory.Read(IntPtr.Add(activeObject, OffsetList.WowObjectGuid.ToInt32()), out ulong guid))
            {
                return new WowObject()
                {
                    BaseAddress = activeObject,
                    DescriptorAddress = descriptorAddress,
                    Guid = guid,
                    Type = wowObjectType
                };
            }
            return null;
        }

        public WowUnit ReadWowUnit(IntPtr activeObject, WowObjectType wowObjectType = WowObjectType.Unit)
        {
            WowObject wowObject = ReadWowObject(activeObject, wowObjectType);

            if (wowObject != null
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorTargetGuid.ToInt32()), out ulong targetGuid)
                && XMemory.ReadStruct(IntPtr.Add(activeObject, OffsetList.WowUnitPosition.ToInt32()), out WowPosition wowPosition)
                && XMemory.Read(IntPtr.Add(activeObject, OffsetList.PlayerRotation.ToInt32()), out float rotation)
                && XMemory.Read(IntPtr.Add(activeObject, OffsetList.IsAutoAttacking.ToInt32()), out int isAutoAttacking)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorFactionTemplate.ToInt32()), out int factionTemplate)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorUnitFlags.ToInt32()), out BitVector32 unitFlags)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorUnitFlagsDynamic.ToInt32()), out BitVector32 dynamicUnitFlags)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorHealth.ToInt32()), out int health)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorMaxHealth.ToInt32()), out int maxHealth)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorMana.ToInt32()), out int mana)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorMaxMana.ToInt32()), out int maxMana)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorRage.ToInt32()), out int rage)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorMaxRage.ToInt32()), out int maxRage)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorEnergy.ToInt32()), out int energy)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorMaxEnergy.ToInt32()), out int maxEnergy)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorRuneenergy.ToInt32()), out int runeenergy)
                //&& XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorMaxRuneenergy.ToInt32()), out int maxRuneenergy)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.DescriptorLevel.ToInt32()), out int level)
                && XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, OffsetList.CurrentlyCastingSpellId.ToInt32()), out int currentlyCastingSpellId)
                && XMemory.Read(IntPtr.Add(activeObject, OffsetList.CurrentlyChannelingSpellId.ToInt32()), out int currentlyChannelingSpellId))
            {
                return new WowUnit()
                {
                    BaseAddress = activeObject,
                    DescriptorAddress = wowObject.DescriptorAddress,
                    Guid = wowObject.Guid,
                    Type = wowObjectType,
                    Name = ReadUnitName(activeObject, wowObject.Guid),
                    TargetGuid = targetGuid,
                    Position = wowPosition,
                    Rotation = rotation,
                    FactionTemplate = factionTemplate,
                    UnitFlags = unitFlags,
                    UnitFlagsDynamic = dynamicUnitFlags,
                    Health = health,
                    MaxHealth = maxHealth,
                    Mana = mana,
                    MaxMana = maxMana,
                    Energy = energy,
                    MaxEnergy = maxEnergy,
                    Rage = rage / 10,
                    MaxRage = maxRage / 10,
                    Runeenergy = runeenergy,
                    MaxRuneenergy = 100,
                    Level = level,
                    IsAutoAttacking = isAutoAttacking == 1,
                    CurrentlyCastingSpellId = currentlyCastingSpellId,
                    CurrentlyChannelingSpellId = currentlyChannelingSpellId
                };
            }
            return null;
        }

        public WowPlayer ReadWowPlayer(IntPtr activeObject, WowObjectType wowObjectType = WowObjectType.Player)
        {
            WowUnit wowUnit = ReadWowUnit(activeObject, wowObjectType);

            if (wowUnit != null)
            {
                WowPlayer player = new WowPlayer()
                {
                    BaseAddress = activeObject,
                    DescriptorAddress = wowUnit.DescriptorAddress,
                    Guid = wowUnit.Guid,
                    Type = wowObjectType,
                    Name = ReadPlayerName(wowUnit.Guid),
                    TargetGuid = wowUnit.TargetGuid,
                    Position = wowUnit.Position,
                    Rotation = wowUnit.Rotation,
                    FactionTemplate = wowUnit.FactionTemplate,
                    UnitFlags = wowUnit.UnitFlags,
                    UnitFlagsDynamic = wowUnit.UnitFlagsDynamic,
                    Health = wowUnit.Health,
                    MaxHealth = wowUnit.MaxHealth,
                    Mana = wowUnit.Mana,
                    MaxMana = wowUnit.MaxMana,
                    Energy = wowUnit.Energy,
                    MaxEnergy = wowUnit.MaxEnergy,
                    Rage = wowUnit.Rage,
                    MaxRage = wowUnit.MaxRage,
                    Runeenergy = wowUnit.Runeenergy,
                    MaxRuneenergy = wowUnit.MaxRuneenergy,
                    Level = wowUnit.Level,
                    IsAutoAttacking = wowUnit.IsAutoAttacking,
                    CurrentlyCastingSpellId = wowUnit.CurrentlyCastingSpellId,
                    CurrentlyChannelingSpellId = wowUnit.CurrentlyChannelingSpellId
                };

                if (PlayerGuid != 0 && wowUnit.Guid == PlayerGuid)
                {
                    if (XMemory.Read(IntPtr.Add(wowUnit.DescriptorAddress, OffsetList.DescriptorExp.ToInt32()), out int exp)
                        && XMemory.Read(IntPtr.Add(wowUnit.DescriptorAddress, OffsetList.DescriptorMaxExp.ToInt32()), out int maxExp)
                        && XMemory.Read(OffsetList.Race, out byte pRace)
                        && XMemory.Read(OffsetList.Class, out byte pClass))
                    {
                        player.Exp = exp;
                        player.MaxExp = maxExp;
                        player.Race = (WowRace)pRace;
                        player.Class = (WowClass)pClass;
                    }
                    Player = player;
                }
                return player;
            }
            return null;
        }

        private string ReadPlayerName(ulong guid)
        {
            if (CacheManager.NameCache.ContainsKey(guid)) return CacheManager.NameCache[guid];

            uint shortGuid;
            uint offset;

            XMemory.Read(IntPtr.Add(OffsetList.NameStore, OffsetList.NameMask.ToInt32()), out uint playerMask);
            XMemory.Read(IntPtr.Add(OffsetList.NameStore, OffsetList.NameBase.ToInt32()), out uint playerBase);

            shortGuid = (uint)guid & 0xfffffff;
            offset = 12 * (playerMask & shortGuid);

            XMemory.Read(new IntPtr(playerBase + offset + 8), out uint current);
            XMemory.Read(new IntPtr(playerBase + offset), out offset);

            if ((current & 0x1) == 0x1) { return ""; }
            XMemory.Read(new IntPtr(current), out uint testGuid);

            while (testGuid != shortGuid)
            {
                XMemory.Read(new IntPtr(current + offset + 4), out current);

                if ((current & 0x1) == 0x1) { return ""; }
                XMemory.Read(new IntPtr(current), out testGuid);
            }

            XMemory.ReadString(IntPtr.Add(new IntPtr(current), OffsetList.NameString.ToInt32()), Encoding.ASCII, out string name, 12);
            if (name.Length > 0) CacheManager.NameCache.Add(guid, name);
            return name;
        }

        private string ReadUnitName(IntPtr activeObject, ulong guid)
        {
            if (CacheManager.NameCache.ContainsKey(guid)) return CacheManager.NameCache[guid];
            try
            {
                XMemory.Read(IntPtr.Add(activeObject, 0x964), out uint objName);
                XMemory.Read(IntPtr.Add(new IntPtr(objName), 0x05C), out objName);

                XMemory.ReadString(new IntPtr(objName), Encoding.ASCII, out string name, 32);
                if (name.Length > 0) CacheManager.NameCache.Add(guid, name);
                return name;
            }
            catch { return "unknown"; }
        }

        public List<ulong> ReadPartymemberGuids()
        {
            List<ulong> partymemberGuids = new List<ulong>();

            if (XMemory.Read(OffsetList.PartyPlayer1, out ulong partyMember1)
                && XMemory.Read(OffsetList.PartyPlayer2, out ulong partyMember2)
                && XMemory.Read(OffsetList.PartyPlayer3, out ulong partyMember3)
                && XMemory.Read(OffsetList.PartyPlayer4, out ulong partyMember4))
            {
                partymemberGuids.Add(partyMember1);
                partymemberGuids.Add(partyMember2);
                partymemberGuids.Add(partyMember3);
                partymemberGuids.Add(partyMember4);

                // try to add raidmembers
                for (uint p = 0; p < 40; p++)
                {
                    try
                    {
                        IntPtr address = IntPtr.Add(OffsetList.RaidGroupStart, (int)(p * OffsetList.RaidGroupPlayer.ToInt32()));
                        if (XMemory.Read(address, out ulong guid))
                        {
                            if (!partymemberGuids.Contains(guid))
                            {
                                partymemberGuids.Add(guid);
                            }
                        }
                    }
                    catch { }
                }
            }
            return partymemberGuids;
        }

        public ulong ReadPartyLeaderGuid()
        {
            ulong partyleaderGuid;
            if (XMemory.Read(OffsetList.RaidLeader, out partyleaderGuid))
            {
                if (partyleaderGuid == 0
                    && XMemory.Read(OffsetList.PartyLeader, out partyleaderGuid))
                {
                }
            }
            return partyleaderGuid;
        }
    }
}
