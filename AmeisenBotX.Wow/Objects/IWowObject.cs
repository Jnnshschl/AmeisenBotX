using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Offsets;
using System;

namespace AmeisenBotX.Wow.Objects
{
    public interface IWowObject
    {
        IntPtr BaseAddress { get; }

        IntPtr DescriptorAddress { get; }

        int EntryId { get; }

        ulong Guid { get; }

        Vector3 Position { get; }

        float Scale { get; }

        WowObjectType Type { get; }

        public float DistanceTo(IWowObject b)
        {
            return Position.GetDistance(b.Position);
        }

        public float DistanceTo(Vector3 b)
        {
            return Position.GetDistance(b);
        }

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
            return Type == WowObjectType.DynamicObject;
        }

        public bool IsGameobject()
        {
            return Type == WowObjectType.GameObject;
        }

        public bool IsInRange(IWowObject b, float range)
        {
            return DistanceTo(b) < range;
        }

        public bool IsInRange(Vector3 b, float range)
        {
            return DistanceTo(b) < range;
        }

        public bool IsPlayer()
        {
            return Type == WowObjectType.Player;
        }

        public bool IsUnit()
        {
            return Type == WowObjectType.Unit;
        }

        void Update(IMemoryApi memoryApi, IOffsetList offsetList);
    }
}