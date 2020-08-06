using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObject.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
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
            Auras = new WowAura[40];
        }

        public WowAura[] Auras { get; set; }

        public int AuraCount { get; set; }

        public WowClass Class { get; set; }

        public float CombatReach { get; set; }

        public int CurrentlyCastingSpellId { get; set; }

        public int CurrentlyChannelingSpellId { get; set; }

        public int DisplayId { get; set; }

        public int Energy { get; set; }

        public double EnergyPercentage => ReturnPercentage(Energy, MaxEnergy);

        public int FactionTemplate { get; set; }

        public WowGender Gender { get; set; }

        public int Health { get; set; }

        public double HealthPercentage => ReturnPercentage(Health, MaxHealth);

        public bool IsAuctioneer => NpcFlags[(int)WowUnitNpcFlags.Auctioneer];

        public bool IsAutoAttacking { get; set; }

        public bool IsBanker => NpcFlags[(int)WowUnitNpcFlags.Banker];

        public bool IsBattlemaster => NpcFlags[(int)WowUnitNpcFlags.Battlemaster];

        public bool IsCasting => CurrentlyCastingSpellId > 0 || CurrentlyChannelingSpellId > 0;

        public bool IsClasstrainer => NpcFlags[(int)WowUnitNpcFlags.ClassTrainer];

        public bool IsConfused => UnitFlags[(int)WowUnitFlags.Confused];

        public bool IsDazed => UnitFlags[(int)WowUnitFlags.Dazed];

        public bool IsDead => (Health == 0 || UnitFlagsDynamic[(int)WowUnitDynamicFlags.Dead]) && !UnitFlags2[(int)WowUnitFlags2.FeignDeath];

        public bool IsDisarmed => UnitFlags[(int)WowUnitFlags.Disarmed];

        public bool IsFleeing => UnitFlags[(int)WowUnitFlags.Fleeing];

        public bool IsFlightmaster => NpcFlags[(int)WowUnitNpcFlags.Flightmaster];

        public bool IsFoodVendor => NpcFlags[(int)WowUnitNpcFlags.FoodVendor];

        public bool IsGeneralGoodsVendor => NpcFlags[(int)WowUnitNpcFlags.GeneralGoodsVendor];

        public bool IsGossip => NpcFlags[(int)WowUnitNpcFlags.Gossip];

        public bool IsGuard => NpcFlags[(int)WowUnitNpcFlags.Guard];

        public bool IsGuildbanker => NpcFlags[(int)WowUnitNpcFlags.Guildbanker];

        public bool IsInCombat => UnitFlags[(int)WowUnitFlags.Combat];

        public bool IsInfluenced => UnitFlags[(int)WowUnitFlags.Influenced];

        public bool IsInnkeeper => NpcFlags[(int)WowUnitNpcFlags.Innkeeper];

        public bool IsInTaxiFlight => UnitFlags[(int)WowUnitFlags.TaxiFlight];

        public bool IsLootable => UnitFlagsDynamic[(int)WowUnitDynamicFlags.Lootable];

        public bool IsLooting => UnitFlags[(int)WowUnitFlags.Looting];

        public bool IsMounted => UnitFlags[(int)WowUnitFlags.Mounted];

        public bool IsNoneNpc => NpcFlags[(int)WowUnitNpcFlags.None];

        public bool IsNotAttackable => UnitFlags[(int)WowUnitFlags.NotAttackable];

        public bool IsNotSelectable => UnitFlags[(int)WowUnitFlags.NotSelectable];

        public bool IsPetInCombat => UnitFlags[(int)WowUnitFlags.PetInCombat];

        public bool IsPetition => NpcFlags[(int)WowUnitNpcFlags.Petitioner];

        public bool IsPlayerControlled => UnitFlags[(int)WowUnitFlags.PlayerControlled];

        public bool IsPlusMob => UnitFlags[(int)WowUnitFlags.PlusMob];

        public bool IsPoisonVendor => NpcFlags[(int)WowUnitNpcFlags.PoisonVendor];

        public bool IsPossessed => UnitFlags[(int)WowUnitFlags.Possessed];

        public bool IsProfessionTrainer => NpcFlags[(int)WowUnitNpcFlags.ProfessionTrainer];

        public bool IsPvpFlagged => UnitFlags[(int)WowUnitFlags.PvPFlagged];

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

        public int Level { get; set; }

        public int Mana { get; set; }

        public double ManaPercentage => ReturnPercentage(Mana, MaxMana);

        public int MaxEnergy { get; set; }

        public int MaxHealth { get; set; }

        public int MaxMana { get; set; }

        public int MaxRage { get; set; }

        public int MaxRuneenergy { get; set; }

        public string Name { get; set; }

        public BitVector32 NpcFlags { get; set; }

        public WowPowertype PowerType { get; set; }

        public WowRace Race { get; set; }

        public int Rage { get; set; }

        public double RagePercentage => ReturnPercentage(Rage, MaxRage);

        public float Rotation { get; set; }

        public int Runeenergy { get; set; }

        public double RuneenergyPercentage => ReturnPercentage(Runeenergy, MaxRuneenergy);

        public ulong SummonedByGuid { get; set; }

        public ulong TargetGuid { get; set; }

        public BitVector32 UnitFlags { get; set; }

        public BitVector32 UnitFlags2 { get; set; }

        public BitVector32 UnitFlagsDynamic { get; set; }

        public bool HasBuffById(int spellId)
        {
            return Auras != null && Auras.Any(e => e.SpellId == spellId);
        }

        public bool HasBuffByName(string name)
        {
            return Auras != null && Auras.Any(e => e.Name == name);
        }

        public bool IsInMeleeRange(WowUnit wowUnit) => wowUnit != null && Position.GetDistance(wowUnit.Position) < Math.Max(4.5, CombatReach + wowUnit.CombatReach + 1);

        public override string ToString()
        {
            return $"Unit: [{Guid}] {Name} lvl. {Level}";
        }

        public WowUnit UpdateRawWowUnit()
        {
            UpdateRawWowObject();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowUnit rawWowUnit))
            {
                Class = (WowClass)((rawWowUnit.Bytes0 >> 8) & 0xFF);
                CombatReach= rawWowUnit.CombatReach;
                DisplayId= rawWowUnit.DisplayId;
                Energy = rawWowUnit.Power4;
                FactionTemplate= rawWowUnit.FactionTemplate;
                Gender = (WowGender)((rawWowUnit.Bytes0 >> 16) & 0xFF);
                Health = rawWowUnit.Health;
                Level = rawWowUnit.Level;
                Mana = rawWowUnit.Power1;
                MaxEnergy = rawWowUnit.MaxPower4;
                MaxHealth = rawWowUnit.MaxHealth;
                MaxMana = rawWowUnit.MaxPower1;
                MaxRage = rawWowUnit.MaxPower2 / 10;
                MaxRuneenergy = rawWowUnit.MaxPower7 / 10;
                NpcFlags = rawWowUnit.NpcFlags;
                PowerType = (WowPowertype)((rawWowUnit.Bytes0 >> 24) & 0xFF);
                Race = (WowRace)((rawWowUnit.Bytes0 >> 0) & 0xFF);
                Rage = rawWowUnit.Power2 / 10;
                Runeenergy = rawWowUnit.Power7 / 10;
                SummonedByGuid = rawWowUnit.SummonedBy;
                TargetGuid = rawWowUnit.Target;
                UnitFlags = rawWowUnit.Flags1;
                UnitFlags2 = rawWowUnit.Flags2;
                UnitFlagsDynamic = rawWowUnit.DynamicFlags;
            }

            if (WowInterface.I.XMemory.ReadStruct(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowUnitPosition), out Vector3 position))
            {
                Position = position;
            }

            if (WowInterface.I.XMemory.Read(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowUnitRotation), out float rotation))
            {
                Rotation = rotation;
            }

            if (WowInterface.I.XMemory.Read(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowUnitIsAutoAttacking), out int isAutoAttacking))
            {
                IsAutoAttacking = isAutoAttacking == 1;
            }

            if (WowInterface.I.XMemory.Read(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.CurrentlyCastingSpellId), out int castingId))
            {
                CurrentlyCastingSpellId = castingId;
            }

            if (WowInterface.I.XMemory.Read(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.CurrentlyChannelingSpellId), out int channelingId))
            {
                CurrentlyChannelingSpellId = channelingId;
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
                return value / (double)max * 100.0;
            }
        }
    }
}