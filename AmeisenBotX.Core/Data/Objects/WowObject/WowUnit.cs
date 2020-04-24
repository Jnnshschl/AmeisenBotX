using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowUnit : WowObject
    {
        public WowUnit(IntPtr baseAddress, WowObjectType type) : base(baseAddress, type)
        {
        }

        public List<WowAura> Auras { get; set; }

        public WowClass Class => Enum.IsDefined(typeof(WowClass), (WowClass)((RawWowUnit.Bytes0 >> 8) & 0xFF)) ? (WowClass)((RawWowUnit.Bytes0 >> 8) & 0xFF) : WowClass.Unknown;

        public float CombatReach => RawWowUnit.CombatReach;

        public int CurrentlyCastingSpellId => RawWowUnit.ChannelSpell;

        public int CurrentlyChannelingSpellId => RawWowUnit.ChannelSpell;

        public int Energy => RawWowUnit.Power3;

        public double EnergyPercentage => ReturnPercentage(Energy, MaxEnergy);

        public int FactionTemplate => RawWowUnit.FactionTemplate;

        public WowGender Gender => Enum.IsDefined(typeof(WowGender), (WowGender)((RawWowUnit.Bytes0 >> 16) & 0xFF)) ? (WowGender)((RawWowUnit.Bytes0 >> 16) & 0xFF) : WowGender.Unknown;

        public int Health => RawWowUnit.Health;

        public double HealthPercentage => ReturnPercentage(Health, MaxHealth);

        public bool IsAuctioneer => NpcFlags[(int)WowUnitNpcFlags.Auctioneer];

        public bool IsAutoAttacking { get; set; }

        public bool IsBanker => NpcFlags[(int)WowUnitNpcFlags.Banker];

        public bool IsBattlemaster => NpcFlags[(int)WowUnitNpcFlags.Battlemaster];

        public bool IsCasting => CurrentlyCastingSpellId > 0 || CurrentlyChannelingSpellId > 0;

        public bool IsClasstrainer => NpcFlags[(int)WowUnitNpcFlags.ClassTrainer];

        public bool IsConfused => UnitFlags[(int)WowUnitFlags.Confused];

        public bool IsDazed => UnitFlags[(int)WowUnitFlags.Dazed];

        public bool IsDead => Health == 0 || UnitFlagsDynamic[(int)WowUnitDynamicFlags.Dead];

        public bool IsDisarmed => UnitFlags[(int)WowUnitFlags.Disarmed];

        public bool IsFleeing => UnitFlags[(int)WowUnitFlags.Fleeing];

        public bool IsFlightmaster => NpcFlags[(int)WowUnitNpcFlags.Flightmaster];

        public bool IsFoodVendor => NpcFlags[(int)WowUnitNpcFlags.FoodVendor];

        public bool IsGeneralGoodsVendor => NpcFlags[(int)WowUnitNpcFlags.GeneralGoodsVendor];

        public bool IsGossip => NpcFlags[(int)WowUnitNpcFlags.Gossip];

        public bool IsGuard => NpcFlags[(int)WowUnitNpcFlags.Guard];

        public bool IsGuildbanker => NpcFlags[(int)WowUnitNpcFlags.Guildbanker];

        public bool IsInCombat => UnitFlags[(int)WowUnitFlags.Combat];

        public bool IsInFlightmasterFlight => UnitFlags[(int)WowUnitFlags.FlightmasterFlight];

        public bool IsInnkeeper => NpcFlags[(int)WowUnitNpcFlags.Innkeeper];

        public bool IsLootable => UnitFlagsDynamic[(int)WowUnitDynamicFlags.Lootable];

        public bool IsLooting => UnitFlags[(int)WowUnitFlags.Looting];

        public bool IsMounted => UnitFlags[(int)WowUnitFlags.Mounted];

        public bool IsNoneNpc => NpcFlags[(int)WowUnitNpcFlags.None];

        public bool IsNotAttackable => UnitFlags[(int)WowUnitFlags.NotAttackable];

        public bool IsPetInCombat => UnitFlags[(int)WowUnitFlags.PetInCombat];

        public bool IsPetition => NpcFlags[(int)WowUnitNpcFlags.Petitioner];

        public bool IsPoisonVendor => NpcFlags[(int)WowUnitNpcFlags.PoisonVendor];

        public bool IsProfessionTrainer => NpcFlags[(int)WowUnitNpcFlags.ProfessionTrainer];

        public bool IsPvpFlagged => UnitFlags[(int)WowUnitFlags.PvpFlagged];

        public bool IsQuestgiver => NpcFlags[(int)WowUnitNpcFlags.Questgiver];

        public bool IsReagentVendor => NpcFlags[(int)WowUnitNpcFlags.ReagentVendor];

        public bool IsReferAFriendLinked => UnitFlagsDynamic[(int)WowUnitDynamicFlags.ReferAFriendLinked];

        public bool IsRepairVendor => NpcFlags[(int)WowUnitNpcFlags.RepairVendor];

        public bool IsSilenced => UnitFlags[(int)WowUnitFlags.Silenced];

        public bool IsSitting => UnitFlags[(int)WowUnitFlags.Sitting];

        public bool IsSkinnable => UnitFlags[(int)WowUnitFlags.Skinnable];

        public bool IsSpecialInfo => UnitFlagsDynamic[(int)WowUnitDynamicFlags.SpecialInfo];

        public bool IsSpellclick => NpcFlags[(int)WowUnitNpcFlags.Spellclick];

        public bool IsSpiritguide => NpcFlags[(int)WowUnitNpcFlags.Spiritguide];

        public bool IsSpirithealer => NpcFlags[(int)WowUnitNpcFlags.Spirithealer];

        public bool IsStablemaster => NpcFlags[(int)WowUnitNpcFlags.Stablemaster];

        public bool IsTabarddesigner => NpcFlags[(int)WowUnitNpcFlags.Tabarddesigner];

        public bool IsTaggedByMe => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TaggedByMe];

        public bool IsTaggedByOther => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TaggedByOther];

        public bool IsTappedByThreat => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TappedByThreat];

        public bool IsTotem => UnitFlags[(int)WowUnitFlags.Totem];

        public bool IsTrackedUnit => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TrackUnit];

        public bool IsTrainer => NpcFlags[(int)WowUnitNpcFlags.Trainer];

        public bool IsVendor => NpcFlags[(int)WowUnitNpcFlags.Vendor];

        public int Level => RawWowUnit.Level;

        public int Mana => RawWowUnit.Power1;

        public double ManaPercentage => ReturnPercentage(Mana, MaxMana);

        public int MaxEnergy => RawWowUnit.MaxPower3;

        public int MaxHealth => RawWowUnit.MaxHealth;

        public int MaxMana => RawWowUnit.MaxPower1;

        public int MaxRage => RawWowUnit.MaxPower2 / 10;

        public int MaxRuneenergy => RawWowUnit.MaxPower7 / 10;

        public string Name { get; set; }

        public BitVector32 NpcFlags => RawWowUnit.NpcFlags;

        public WowPowertype PowerType => Enum.IsDefined(typeof(WowPowertype), (WowPowertype)((RawWowUnit.Bytes0 >> 24) & 0xFF)) ? (WowPowertype)((RawWowUnit.Bytes0 >> 24) & 0xFF) : WowPowertype.Unknown;

        public WowRace Race => Enum.IsDefined(typeof(WowRace), (WowRace)((RawWowUnit.Bytes0 >> 0) & 0xFF)) ? (WowRace)((RawWowUnit.Bytes0 >> 0) & 0xFF) : WowRace.Unknown;

        public int Rage => RawWowUnit.Power2 / 10;

        public double RagePercentage => ReturnPercentage(Rage, MaxRage);

        public float Rotation { get; set; }

        public int Runeenergy => RawWowUnit.Power7 / 10;

        public double RuneenergyPercentage => ReturnPercentage(Runeenergy, MaxRuneenergy);

        public ulong TargetGuid => RawWowUnit.Target;

        public BitVector32 UnitFlags => RawWowUnit.Flags1;

        public BitVector32 UnitFlagsDynamic => RawWowUnit.DynamicFlags;

        private RawWowUnit RawWowUnit { get; set; }

        public bool HasBuffByName(string name)
            => Auras != null && Auras.Any(e => e.Name == name);

        public override string ToString()
            => $"Unit: [{Guid}] {Name} lvl. {Level}";

        public WowUnit UpdateRawWowUnit(XMemory xMemory)
        {
            UpdateRawWowObject(xMemory);

            if (xMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowUnit rawWowUnit))
            {
                RawWowUnit = rawWowUnit;
            }

            return this;
        }

        internal double ReturnPercentage(int value, int max)
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