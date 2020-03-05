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
        }

        public event ObjectUpdateComplete OnObjectUpdateComplete;

        public bool IsWorldLoaded { get; private set; }

        public GameState GameState { get; private set; }

        public WowUnit LastTarget { get; private set; }

        public ulong LastTargetGuid { get; private set; }

        public MapId MapId { get; private set; }

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
                for (uint p = 0; p < 40; ++p)
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

        public WowObject ReadWowObject(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectDescriptor.ToInt32()), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectPosition.ToInt32()), out Vector3 position))
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

        public WowPlayer ReadWowPlayer(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectDescriptor.ToInt32()), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, WowInterface.OffsetList.WowUnitPosition.ToInt32()), out Vector3 position))
            {
                WowPlayer player = new WowPlayer(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position
                };

                player.Name = ReadPlayerName(player.Guid);
                player.UpdateRawWowPlayer(WowInterface.XMemory);

                if (PlayerGuid != 0 && player.Guid == PlayerGuid)
                {
                    if (WowInterface.XMemory.Read(WowInterface.OffsetList.ComboPoints, out byte comboPoints))
                    {
                        player.ComboPoints = comboPoints;
                    }

                    Player = player;
                }

                return player;
            }

            return null;
        }

        public WowUnit ReadWowUnit(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectDescriptor.ToInt32()), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, WowInterface.OffsetList.WowUnitPosition.ToInt32()), out Vector3 position)
                && WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.PlayerRotation.ToInt32()), out float rotation)
                && WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.IsAutoAttacking.ToInt32()), out int isAutoAttacking))
            {
                WowUnit unit = new WowUnit(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position,
                    Rotation = rotation,
                    IsAutoAttacking = isAutoAttacking == 1
                };

                unit.Name = ReadUnitName(activeObject, unit.Guid);
                return unit.UpdateRawWowUnit(WowInterface.XMemory);
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
                case WowObjectType.Dynobject:
                    WowObjects.Add(ReadWowDynobject(baseAddress, wowObjectType));
                    break;

                case WowObjectType.Gameobject:
                    WowObjects.Add(ReadWowGameobject(baseAddress, wowObjectType));
                    break;

                case WowObjectType.Player:
                    WowObjects.Add(ReadWowPlayer(baseAddress, wowObjectType));
                    break;

                case WowObjectType.Unit:
                    WowObjects.Add(ReadWowUnit(baseAddress, wowObjectType));
                    break;

                default:
                    WowObjects.Add(ReadWowObject(baseAddress, wowObjectType));
                    break;
            }
        }

        public void UpdateWowObjects()
        {
            if (WowInterface.XMemory.Read(WowInterface.OffsetList.IsWorldLoaded, out int isWorldLoaded))
            {
                IsWorldLoaded = isWorldLoaded == 1;

                if (!IsWorldLoaded)
                {
                    return;
                }
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

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.MapId, out MapId mapId))
            {
                MapId = mapId;
            }

            if (WowInterface.XMemory.Read(WowInterface.OffsetList.ZoneId, out int zoneId))
            {
                ZoneId = zoneId;
            }

            //if (WowInterface.XMemory.Read(WowInterface.OffsetList.GameState, out GameState gamestate))
            //{
            //    GameState = gamestate;
            //}

            WowObjects.Clear();
            WowInterface.XMemory.Read(WowInterface.OffsetList.ClientConnection, out IntPtr clientConnection);
            WowInterface.XMemory.Read(IntPtr.Add(clientConnection, WowInterface.OffsetList.CurrentObjectManager.ToInt32()), out IntPtr currentObjectManager);

            WowInterface.XMemory.Read(IntPtr.Add(currentObjectManager, WowInterface.OffsetList.FirstObject.ToInt32()), out IntPtr activeObjectBaseAddress);
            WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, WowInterface.OffsetList.WowObjectType.ToInt32()), out int activeObjectType);

            while (isWorldLoaded == 1 && (activeObjectType <= 7 && activeObjectType > 0))
            {
                WowObjectType wowObjectType = (WowObjectType)activeObjectType;
                WowObject obj = wowObjectType switch
                {
                    WowObjectType.Dynobject => ReadWowDynobject(activeObjectBaseAddress, wowObjectType),
                    WowObjectType.Gameobject => ReadWowGameobject(activeObjectBaseAddress, wowObjectType),
                    WowObjectType.Player => ReadWowPlayer(activeObjectBaseAddress, wowObjectType),
                    WowObjectType.Unit => ReadWowUnit(activeObjectBaseAddress, wowObjectType),
                    _ => ReadWowObject(activeObjectBaseAddress, wowObjectType),
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

                WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, WowInterface.OffsetList.NextObject.ToInt32()), out activeObjectBaseAddress);
                WowInterface.XMemory.Read(IntPtr.Add(activeObjectBaseAddress, WowInterface.OffsetList.WowObjectType.ToInt32()), out activeObjectType);
            }

            PartyleaderGuid = ReadPartyLeaderGuid();
            PartymemberGuids = ReadPartymemberGuids();

            OnObjectUpdateComplete?.Invoke(WowObjects);
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
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectDescriptor.ToInt32()), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectPosition.ToInt32()), out Vector3 position))
            {
                WowDynobject dynObject = new WowDynobject(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position
                };

                return dynObject.UpdateRawWowDynobject(WowInterface.XMemory);
            }

            return null;
        }

        private WowGameobject ReadWowGameobject(IntPtr activeObject, WowObjectType wowObjectType)
        {
            if (WowInterface.XMemory.Read(IntPtr.Add(activeObject, WowInterface.OffsetList.WowObjectDescriptor.ToInt32()), out IntPtr descriptorAddress)
                && WowInterface.XMemory.ReadStruct(IntPtr.Add(activeObject, WowInterface.OffsetList.WowGameobjectPosition.ToInt32()), out Vector3 position))
            {
                WowGameobject dynObject = new WowGameobject(activeObject, wowObjectType)
                {
                    DescriptorAddress = descriptorAddress,
                    Position = position
                };

                return dynObject.UpdateRawWowGameobject(WowInterface.XMemory);
            }

            return null;
        }
    }
}