using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Raw;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Data.Objects
{
    public class WowObject
    {
        public WowObject(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress)
        {
            BaseAddress = baseAddress;
            Type = type;
            DescriptorAddress = descriptorAddress;
        }

        public IntPtr BaseAddress { get; private set; }

        public IntPtr DescriptorAddress { get; set; }

        public int EntryId { get; set; }

        public ulong Guid { get; set; }

        public Vector3 Position { get; set; }

        public float Scale { get; set; }

        public WowObjectType Type { get; private set; }

        public float DistanceTo(WowObject b) => Position.GetDistance(b.Position);

        public float DistanceTo(Vector3 b) => Position.GetDistance(b);

        public bool IsContainer()
        {
            return Type == WowObjectType.Container;
        }

        public bool IsCorpse()
        {
            return Type == WowObjectType.Corpse;
        }

        public bool IsDynoject()
        {
            return Type == WowObjectType.Dynobject;
        }

        public bool IsGameobject()
        {
            return Type == WowObjectType.Gameobject;
        }

        public bool IsInRange(WowObject b, float range) => DistanceTo(b) < range;

        public bool IsInRange(Vector3 b, float range) => DistanceTo(b) < range;

        public bool IsPlayer()
        {
            return Type == WowObjectType.Player;
        }

        public bool IsUnit()
        {
            return Type == WowObjectType.Unit;
        }

        public override string ToString()
        {
            return $"Object: {Guid}";
        }

        public virtual void Update(WowInterface wowInterface)
        {
            if (wowInterface.XMemory.Read(DescriptorAddress, out RawWowObject objPtr))
            {
                EntryId = objPtr.EntryId;
                Guid = objPtr.Guid;
                Scale = objPtr.Scale;
            }
        }
    }
}