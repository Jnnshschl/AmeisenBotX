using AmeisenBotX.Core.Data.Enums;
using System.Collections.Specialized;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowUnit : WowObject
    {
        public WowClass Class { get; set; }

        public int CurrentlyCastingSpellId { get; set; }

        public int CurrentlyChannelingSpellId { get; set; }

        public int Energy { get; set; }

        public double EnergyPercentage => ReturnPercentage(Energy, MaxEnergy);

        public int FactionTemplate { get; set; }

        public WowGender Gender { get; set; }

        public int Health { get; set; }

        public double HealthPercentage => ReturnPercentage(Health, MaxHealth);

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

        public double ManaPercentage => ReturnPercentage(Mana, MaxMana);

        public int MaxEnergy { get; set; }

        public int MaxHealth { get; set; }

        public int MaxMana { get; set; }

        public int MaxRage { get; set; }

        public int MaxRuneenergy { get; set; }

        public string Name { get; set; }

        public WowPowertype PowerType { get; set; }

        public WowRace Race { get; set; }

        public int Rage { get; set; }

        public double RagePercentage => ReturnPercentage(Rage, MaxRage);

        public float Rotation { get; set; }

        public int Runeenergy { get; set; }

        public double RuneenergyPercentage => ReturnPercentage(Runeenergy, MaxRuneenergy);

        public ulong TargetGuid { get; set; }

        public BitVector32 UnitFlags { get; set; }

        public BitVector32 UnitFlagsDynamic { get; set; }

        private double ReturnPercentage(int value, int max)
        {
            if (value == 0 || max == 0)
            {
                return 0;
            }
            else
            {
                return (double)value / (double)max * 100.0;
            }
        }
    }
}
