using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.Data
{
    public class ObjectManager : IObjectManager
    {
        private readonly object queryLock = new object();

        private List<WowObject> wowObjects;

        public ObjectManager(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            IsWorldLoaded = true;
            WowObjects = new List<WowObject>();

            EnableClickToMove();
        }

        public event ObjectUpdateComplete OnObjectUpdateComplete;

        public bool IsWorldLoaded { get; private set; }

        public WowUnit LastTarget { get; private set; }

        public ulong LastTargetGuid { get; private set; }

        public int MapId { get; private set; }

        public ulong PartyleaderGuid { get; private set; }

        public List<ulong> PartymemberGuids { get; private set; }

        public List<WowObject> Partymembers
            => WowObjects.Where(e => PartymemberGuids.Contains(e.Guid)).ToList();

        public WowUnit Pet { get; private set; }

        public ulong PetGuid { get; private set; }

        public WowPlayer Player { get; private set; }

        public IntPtr PlayerBase { get; private set; }

        public ulong PlayerGuid { get; private set; }

        public WowUnit Target { get; private set; }

        public ulong TargetGuid { get; private set; }

        public List<WowObject> WowObjects
        {
            get
            {
                lock (queryLock)
                {
                    return wowObjects;
                }
            }

            set
            {
                lock (queryLock)
                {
                    wowObjects = value;
                }
            }
        }

        public int ZoneId { get; private set; }

        private WowInterface WowInterface { get; }

        public IEnumerable<WowPlayer> GetNearEnemies(Vector3 position, double distance)
            => WowObjects.OfType<WowPlayer>()
                .Where(e => e.Guid != PlayerGuid
                && !e.IsDead
                && !e.IsNotAttackable
                && WowInterface.HookManager.GetUnitReaction(Player, e) != WowUnitReaction.Friendly
                && e.Position.GetDistance(position) < distance);

        public IEnumerable<WowPlayer> GetNearFriends(Vector3 position, int distance)
            => WowObjects.OfType<WowPlayer>()
                .Where(e => e.Guid != PlayerGuid
                && !e.IsDead
                && !e.IsNotAttackable
                && WowInterface.HookManager.GetUnitReaction(Player, e) == WowUnitReaction.Friendly
                && e.Position.GetDistance(position) < distance);

        public WowObject GetWowObjectByGuid(ulong guid)
                            => WowObjects.FirstOrDefault(e => e.Guid == guid);

        public T GetWowObjectByGuid<T>(ulong guid) where T : WowObject
            => WowObjects.OfType<T>().FirstOrDefault(e => e.Guid == guid);

        public WowPlayer GetWowPlayerByName(string playername, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
            => WowObjects.OfType<WowPlayer>().FirstOrDefault(e => e.Name.Equals(playername.ToUpper(), stringComparison));

        public ulong ReadPartyLeaderGuid()
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

        public List<ulong> ReadPartymemberGuids()
        {
            List<ulong> partymemberGuids = new List<ulong>();

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.PartyPlayer1, out ulong partyMember1)
                && WowInterface.XMemory.Read(WowInterface.OffsetList.PartyPlayer2, out ulong partyMember2)
                && WowInterface.XMemory.Read(WowInterface.OffsetList.PartyPlayer3, out ulong partyMember3)
                && WowInterface.XMemory.Read(WowInterface.OffsetList.PartyPlayer4, out ulong partyMember4))
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
                        IntPtr address = IntPtr.Add(WowInterface.OffsetList.RaidGroupStart, (int)(p * WowInterface.OffsetList.RaidGroupPlayer.ToInt32()));
                        if (WowInterface.XMemory.Read(address, out ulong guid))
                        {
                            if (!partymemberGuids.Contains(guid))
                            {
                                partymemberGuids.Add(guid);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return partymemberGuids;
        }

        public WowObject ReadWowObject(IntPtr activeObject, WowObjectType wowObjectType = WowObjectType.None)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectDescriptor.ToInt32()), out IntPtr descriptorAddress)
                && WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectGuid.ToInt32()), out ulong guid)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectPosition.ToInt32()), out Vector3 wowPosition))
            {
                return new WowObject()
                {
                    BaseAddress = activeObject,
                    DescriptorAddress = descriptorAddress,
                    Guid = guid,
                    Type = wowObjectType,
                    Position = wowPosition
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
                    CombatReach = wowUnit.CombatReach,
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
                    Race = wowUnit.Race,
                    Class = wowUnit.Class,
                    Gender = wowUnit.Gender,
                    PowerType = wowUnit.PowerType,
                    IsAutoAttacking = wowUnit.IsAutoAttacking,
                    CurrentlyCastingSpellId = wowUnit.CurrentlyCastingSpellId,
                    CurrentlyChannelingSpellId = wowUnit.CurrentlyChannelingSpellId
                };

                if (PlayerGuid != 0 && wowUnit.Guid == PlayerGuid)
                {
                    if (WowInterface.XMemory.Read(IntPtr.Add(wowUnit.DescriptorAddress, WowInterface.OffsetList.DescriptorExp.ToInt32()), out int exp)
                        && WowInterface.XMemory.Read(IntPtr.Add(wowUnit.DescriptorAddress, WowInterface.OffsetList.DescriptorMaxExp.ToInt32()), out int maxExp)
                        && WowInterface.XMemory.Read(WowInterface.OffsetList.ComboPoints, out byte comboPoints))
                    {
                        player.Exp = exp;
                        player.MaxExp = maxExp;
                        player.ComboPoints = comboPoints;
                    }

                    Player = player;
                }

                return player;
            }

            return null;
        }

        public WowUnit ReadWowUnit(IntPtr activeObject, WowObjectType wowObjectType = WowObjectType.Unit)
        {
            WowObject wowObject = ReadWowObject(activeObject, wowObjectType);

            if (wowObject != null
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorTargetGuid.ToInt32()), out ulong targetGuid)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, WowInterface.OffsetList.WowUnitPosition.ToInt32()), out Vector3 wowPosition)
                && WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.PlayerRotation.ToInt32()), out float rotation)
                && WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.IsAutoAttacking.ToInt32()), out int isAutoAttacking)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorCombatReach.ToInt32()), out float combatReach)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorFactionTemplate.ToInt32()), out int factionTemplate)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorUnitFlags.ToInt32()), out BitVector32 unitFlags)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorUnitFlagsDynamic.ToInt32()), out BitVector32 dynamicUnitFlags)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorNpcFlags.ToInt32()), out BitVector32 npcFlags)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorHealth.ToInt32()), out int health)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorMaxHealth.ToInt32()), out int maxHealth)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorMana.ToInt32()), out int mana)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorMaxMana.ToInt32()), out int maxMana)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorRage.ToInt32()), out int rage)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorMaxRage.ToInt32()), out int maxRage)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorEnergy.ToInt32()), out int energy)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorMaxEnergy.ToInt32()), out int maxEnergy)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorRuneenergy.ToInt32()), out int runeenergy)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorMaxRuneenergy.ToInt32()), out int maxRuneenergy)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorLevel.ToInt32()), out int level)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.DescriptorInfoFlags.ToInt32()), out int infoFlags)
                && WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.CurrentlyCastingSpellId.ToInt32()), out int currentlyCastingSpellId)
                && WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.CurrentlyChannelingSpellId.ToInt32()), out int currentlyChannelingSpellId))
            {
                return new WowUnit()
                {
                    BaseAddress = activeObject,
                    CombatReach = combatReach,
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
                    NpcFlags = npcFlags,
                    Energy = energy,
                    MaxEnergy = maxEnergy,
                    Rage = rage / 10,
                    MaxRage = maxRage / 10,
                    Runeenergy = runeenergy / 10,
                    MaxRuneenergy = maxRuneenergy / 10,
                    Level = level,
                    Race = Enum.IsDefined(typeof(WowRace), (WowRace)((infoFlags >> 0) & 0xFF)) ? (WowRace)((infoFlags >> 0) & 0xFF) : WowRace.Unknown,
                    Class = Enum.IsDefined(typeof(WowClass), (WowClass)((infoFlags >> 8) & 0xFF)) ? (WowClass)((infoFlags >> 8) & 0xFF) : WowClass.Unknown,
                    Gender = Enum.IsDefined(typeof(WowGender), (WowGender)((infoFlags >> 16) & 0xFF)) ? (WowGender)((infoFlags >> 16) & 0xFF) : WowGender.Unknown,
                    PowerType = Enum.IsDefined(typeof(WowPowertype), (WowPowertype)((infoFlags >> 24) & 0xFF)) ? (WowPowertype)((infoFlags >> 24) & 0xFF) : WowPowertype.Unknown,
                    IsAutoAttacking = isAutoAttacking == 1,
                    CurrentlyCastingSpellId = currentlyCastingSpellId,
                    CurrentlyChannelingSpellId = currentlyChannelingSpellId
                };
            }

            return null;
        }

        public bool RefreshIsWorldLoaded()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                IsWorldLoaded = isWorldLoaded == 1;
                return IsWorldLoaded;
            }

            return false;
        }

        public void UpdateObject<T>(T wowObject) where T : WowObject
        {
            if (wowObject == null)
            {
                return;
            }

            UpdateObject(wowObject.Type, wowObject.BaseAddress);
        }

        public void UpdateObject(WowObjectType wowObjectType, IntPtr baseAddress)
        {
            WowObjects.RemoveAll(e => e.BaseAddress == baseAddress);
            switch (wowObjectType)
            {
                case WowObjectType.Gameobject:
                    WowObjects.Add(ReadWowGameobject(baseAddress, wowObjectType));
                    break;

                case WowObjectType.Dynobject:
                    WowObjects.Add(ReadWowDynobject(baseAddress, wowObjectType));
                    break;

                case WowObjectType.Unit:
                    WowObjects.Add(ReadWowUnit(baseAddress, wowObjectType));
                    break;

                case WowObjectType.Player:
                    WowObjects.Add(ReadWowPlayer(baseAddress, wowObjectType));
                    break;

                default:
                    WowObjects.Add(ReadWowObject(baseAddress, wowObjectType));
                    break;
            }
        }

        public void UpdateWowObjects()
        {
            if (!WowInterface.XMemory.Read(WowInterface.OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                IsWorldLoaded = isWorldLoaded == 1;
                return;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.PlayerGuid, out ulong playerGuid))
            {
                PlayerGuid = playerGuid;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.TargetGuid, out ulong targetGuid))
            {
                TargetGuid = targetGuid;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.TargetGuid, out ulong lastTargetGuid))
            {
                LastTargetGuid = lastTargetGuid;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.PetGuid, out ulong petGuid))
            {
                PetGuid = petGuid;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.TargetGuid, out IntPtr playerbase))
            {
                PlayerBase = playerbase;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.MapId, out int mapId))
            {
                MapId = mapId;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.ZoneId, out int zoneId))
            {
                ZoneId = zoneId;
            }

            WowObjects = new List<WowObject>();
            WowInterface.XMemory.Read(WowInterface.OffsetList.ClientConnection, out IntPtr clientConnection);
            WowInterface.XMemory.Read(IntPtr.Add(clientConnection, WowInterface.OffsetList.CurrentObjectManager.ToInt32()), out IntPtr currentObjectManager);

            WowInterface.XMemory.Read(IntPtr.Add(currentObjectManager, WowInterface.OffsetList.FirstObject.ToInt32()), out IntPtr activeObject);
            WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectType.ToInt32()), out int objectType);

            while (isWorldLoaded == 1 && (objectType <= 7 && objectType > 0))
            {
                WowObjectType wowObjectType = (WowObjectType)objectType;
                WowObject obj = wowObjectType switch
                {
                    WowObjectType.Gameobject => ReadWowGameobject(activeObject, wowObjectType),
                    WowObjectType.Dynobject => ReadWowDynobject(activeObject, wowObjectType),
                    WowObjectType.Unit => ReadWowUnit(activeObject, wowObjectType),
                    WowObjectType.Player => ReadWowPlayer(activeObject, wowObjectType),
                    _ => ReadWowObject(activeObject, wowObjectType),
                };

                WowObjects.Add(obj);

                if (obj.Guid == TargetGuid)
                {
                    Target = (WowUnit)obj;
                }

                if (obj.Guid == PetGuid)
                {
                    Pet = (WowUnit)obj;
                }

                if (obj.Guid == LastTargetGuid)
                {
                    LastTarget = (WowUnit)obj;
                }

                WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.NextObject.ToInt32()), out activeObject);
                WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectType.ToInt32()), out objectType);
            }

            PartyleaderGuid = ReadPartyLeaderGuid();
            PartymemberGuids = ReadPartymemberGuids();

            OnObjectUpdateComplete?.Invoke(WowObjects);
        }

        private void EnableClickToMove()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.ClickToMovePointer, out IntPtr ctmPointer)
                && WowInterface.XMemory.Read(IntPtr.Add(ctmPointer, WowInterface.OffsetList.ClickToMoveEnabled.ToInt32()), out int ctmEnabled))
            {
                if (ctmEnabled != 1)
                {
                    WowInterface.XMemory.Write(IntPtr.Add(ctmPointer, WowInterface.OffsetList.ClickToMoveEnabled.ToInt32()), 1);
                }
            }
        }

        private string ReadPlayerName(ulong guid)
        {
            if (WowInterface.BotCache.TryGetName(guid, out string cachedName))
            {
                return cachedName;
            }

            uint shortGuid;
            uint offset;

            WowInterface.XMemory.Read(IntPtr.Add(WowInterface.OffsetList.NameStore, WowInterface.OffsetList.NameMask.ToInt32()), out uint playerMask);
            WowInterface.XMemory.Read(IntPtr.Add(WowInterface.OffsetList.NameStore, WowInterface.OffsetList.NameBase.ToInt32()), out uint playerBase);

            shortGuid = (uint)guid & 0xfffffff;
            offset = 12 * (playerMask & shortGuid);

            WowInterface.XMemory.Read(new IntPtr(playerBase + offset + 8), out uint current);
            WowInterface.XMemory.Read(new IntPtr(playerBase + offset), out offset);

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

            WowInterface.XMemory.ReadString(IntPtr.Add(new IntPtr(current), WowInterface.OffsetList.NameString.ToInt32()), Encoding.UTF8, out string name, 16);

            if (name.Length > 0)
            {
                WowInterface.BotCache.CacheName(guid, name);
            }

            return name;
        }

        private string ReadUnitName(IntPtr activeObject, ulong guid)
        {
            if (WowInterface.BotCache.TryGetName(guid, out string cachedName))
            {
                return cachedName;
            }

            try
            {
                WowInterface.XMemory.Read(IntPtr.Add(activeObject, 0x964), out uint objName);
                WowInterface.XMemory.Read(IntPtr.Add(new IntPtr(objName), 0x05C), out objName);

                WowInterface.XMemory.ReadString(new IntPtr(objName), Encoding.UTF8, out string name, 32);

                if (name.Length > 0)
                {
                    WowInterface.BotCache.CacheName(guid, name);
                }

                return name;
            }
            catch
            {
                return "unknown";
            }
        }

        private WowDynobject ReadWowDynobject(IntPtr activeObject, WowObjectType wowObjectType)
        {
            WowObject wowObject = ReadWowObject(activeObject, wowObjectType);

            if (wowObject != null
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.WowDynobjectCasterGuid.ToInt32()), out ulong casterGuid)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.WowDynobjectSpellId.ToInt32()), out int spellId)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.WowDynobjectRadius.ToInt32()), out float radius)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.WowDynobjectFacing.ToInt32()), out float facing))
            {
                return new WowDynobject()
                {
                    BaseAddress = activeObject,
                    DescriptorAddress = wowObject.DescriptorAddress,
                    Guid = wowObject.Guid,
                    Position = wowObject.Position,
                    Type = wowObjectType,
                    CasterGuid = casterGuid,
                    SpellId = spellId,
                    Radius = radius,
                    Facing = facing
                };
            }

            return null;
        }

        private WowGameobject ReadWowGameobject(IntPtr activeObject, WowObjectType wowObjectType)
        {
            WowObject wowObject = ReadWowObject(activeObject, wowObjectType);

            if (wowObject != null
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.WowGameobjectType.ToInt32()), out WowGameobjectType gameobjectType)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.WowGameobjectLevel.ToInt32()), out int level)
                && WowInterface.XMemory.Read(IntPtr.Add(wowObject.DescriptorAddress, WowInterface.OffsetList.WowGameobjectDisplayId.ToInt32()), out int displayId)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, WowInterface.OffsetList.WowGameobjectPosition.ToInt32()), out Vector3 wowPosition))
            {
                return new WowGameobject()
                {
                    BaseAddress = activeObject,
                    DescriptorAddress = wowObject.DescriptorAddress,
                    Guid = wowObject.Guid,
                    Position = wowPosition,
                    Type = wowObjectType,
                    DisplayId = displayId,
                    Level = level,
                    GameobjectType = gameobjectType,
                };
            }

            return null;
        }
    }
}