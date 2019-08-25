using System.Collections.Specialized;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowUnit : WowObject
    {
        public int CurrentlyCastingSpellId { get; set; }
        public int CurrentlyChannelingSpellId { get; set; }
        public int Energy { get; set; }
        public int FactionTemplate { get; set; }
        public int Health { get; set; }
        public bool IsAutoAttacking { get; set; }
        public bool IsConfused => UnitFlags[(int)WowUnitFlags.Confused];
        public bool IsDazed => UnitFlags[(int)WowUnitFlags.Dazed];
        public bool IsDead => Health == 0 || UnitFlagsDynamic[(int)WowUnitDynamicFlags.Dead];
        public bool IsDisarmed => UnitFlags[(int)WowUnitFlags.Disarmed];
        public bool IsFleeing => UnitFlags[(int)WowUnitFlags.Fleeing];
        public bool IsInCombat => UnitFlags[(int)WowUnitFlags.Combat];
        public bool IsInFlightmasterFlight => UnitFlags[(int)WowUnitFlags.FlightmasterFlight];
        public bool IsLootable => UnitFlagsDynamic[(int)WowUnitDynamicFlags.Lootable];
        public bool IsLooting => UnitFlags[(int)WowUnitFlags.Looting];
        public bool IsMounted => UnitFlags[(int)WowUnitFlags.Mounted];
        public bool IsNotAttackable => UnitFlags[(int)WowUnitFlags.NotAttackable];
        public bool IsPetInCombat => UnitFlags[(int)WowUnitFlags.PetInCombat];
        public bool IsPvpFlagged => UnitFlags[(int)WowUnitFlags.PvpFlagged];
        public bool IsReferAFriendLinked => UnitFlagsDynamic[(int)WowUnitDynamicFlags.ReferAFriendLinked];
        public bool IsSilenced => UnitFlags[(int)WowUnitFlags.Silenced];
        public bool IsSitting => UnitFlags[(int)WowUnitFlags.Sitting];
        public bool IsSkinnable => UnitFlags[(int)WowUnitFlags.Skinnable];
        public bool IsSpecialInfo => UnitFlagsDynamic[(int)WowUnitDynamicFlags.SpecialInfo];
        public bool IsTaggedByMe => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TaggedByMe];
        public bool IsTaggedByOther => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TaggedByOther];
        public bool IsTappedByThreat => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TappedByThreat];
        public bool IsTotem => UnitFlags[(int)WowUnitFlags.Totem];
        public bool IsTrackedUnit => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TrackUnit];
        public int Level { get; set; }
        public int Mana { get; set; }
        public int MaxEnergy { get; set; }
        public int MaxHealth { get; set; }
        public int MaxMana { get; set; }
        public int MaxRage { get; set; }
        public int MaxRuneenergy { get; set; }
        public string Name { get; set; }

        public int Rage { get; set; }
        public float Rotation { get; set; }
        public int Runeenergy { get; set; }
        public ulong TargetGuid { get; set; }
        public BitVector32 UnitFlags { get; set; }
        public BitVector32 UnitFlagsDynamic { get; set; }
    }
}