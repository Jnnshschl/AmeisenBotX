using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Offsets;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Cache;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.Data.Objects
{
    public class WowUnit : WowObject
    {
        public WowUnit(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public int AuraCount { get; set; }

        public IEnumerable<RawWowAura> Auras { get; set; }

        public WowClass Class { get; set; }

        public float CombatReach { get; set; }

        public int CurrentlyCastingSpellId { get; set; }

        public int CurrentlyChannelingSpellId { get; set; }

        public int DisplayId { get; set; }

        public int Energy { get; set; }

        public double EnergyPercentage { get; set; }

        public int FactionTemplate { get; set; }

        public WowGender Gender { get; set; }

        public int Health { get; set; }

        public double HealthPercentage { get; set; }

        public bool IsAuctioneer => NpcFlags[(int)WowUnitNpcFlags.Auctioneer];

        public bool IsAutoAttacking { get; set; }

        public bool IsBanker => NpcFlags[(int)WowUnitNpcFlags.Banker];

        public bool IsBattlemaster => NpcFlags[(int)WowUnitNpcFlags.Battlemaster];

        public bool IsCasting => CurrentlyCastingSpellId > 0 || CurrentlyChannelingSpellId > 0;

        public bool IsClasstrainer => NpcFlags[(int)WowUnitNpcFlags.ClassTrainer];

        public bool IsConfused => UnitFlags[(int)WowUnitFlags.Confused];

        public bool IsDazed => UnitFlags[(int)WowUnitFlags.Dazed];

        public bool IsDead => (Health == 0 || UnitFlagsDynamic[(int)WowUnitDynamicFlags.Dead]) && !UnitFlags2[(int)WowUnit2Flags.FeignDeath];

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

        public double ManaPercentage { get; set; }

        public int MaxEnergy { get; set; }

        public int MaxHealth { get; set; }

        public int MaxMana { get; set; }

        public int MaxRage { get; set; }

        public int MaxRuneenergy { get; set; }

        public BitVector32 NpcFlags { get; set; }

        public WowPowertype PowerType { get; set; }

        public WowRace Race { get; set; }

        public int Rage { get; set; }

        public double RagePercentage { get; set; }

        public float Rotation { get; set; }

        public int Runeenergy { get; set; }

        public double RuneenergyPercentage { get; set; }

        public ulong SummonedByGuid { get; set; }

        public ulong TargetGuid { get; set; }

        public BitVector32 UnitFlags { get; set; }

        public BitVector32 UnitFlags2 { get; set; }

        public BitVector32 UnitFlagsDynamic { get; set; }

        public static bool IsValidUnit(WowUnit unit)
        {
            return unit != null && !unit.IsNotAttackable;
        }

        public bool HasBuffById(int spellId)
        {
            return Auras != null && Auras.Any(e => e.SpellId == spellId);
        }        

        public bool IsInMeleeRange(WowUnit wowUnit)
        {
            return wowUnit != null && Position.GetDistance(wowUnit.Position) < MathF.Max(4.5f, CombatReach + wowUnit.CombatReach + 1.0f);
        }

        public virtual string ReadName(XMemory xMemory, IOffsetList offsetList)
        {
            if (xMemory.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitName1), out IntPtr objName)
                && xMemory.Read(IntPtr.Add(objName, (int)offsetList.WowUnitName2), out objName)
                && xMemory.ReadString(objName, Encoding.UTF8, out string name))
            {
                return name;
            }

            return "unknown";
        }

        public override string ToString()
        {
            return $"Unit: {Guid} lvl. {Level} Position: {Position} DisplayId: {DisplayId}";
        }

        public override void Update(XMemory xMemory, IOffsetList offsetList)
        {
            base.Update(xMemory, offsetList);

            if (xMemory.Read(DescriptorAddress + RawWowObject.EndOffset, out RawWowUnit objPtr))
            {
                Class = (WowClass)objPtr.Class;
                CombatReach = objPtr.CombatReach;
                DisplayId = objPtr.DisplayId;
                Energy = objPtr.Power4;
                FactionTemplate = objPtr.FactionTemplate;
                Gender = (WowGender)objPtr.Gender;
                Health = objPtr.Health;
                Level = objPtr.Level;
                Mana = objPtr.Power1;
                MaxEnergy = objPtr.MaxPower4;
                MaxHealth = objPtr.MaxHealth;
                MaxMana = objPtr.MaxPower1;
                MaxRage = objPtr.MaxPower2 / 10;
                MaxRuneenergy = objPtr.MaxPower7 / 10;
                NpcFlags = objPtr.NpcFlags;
                PowerType = (WowPowertype)objPtr.PowerType;
                Race = (WowRace)objPtr.Race;
                Rage = objPtr.Power2 / 10;
                Runeenergy = objPtr.Power7 / 10;
                SummonedByGuid = objPtr.SummonedBy;
                TargetGuid = objPtr.Target;
                UnitFlags = objPtr.Flags1;
                UnitFlags2 = objPtr.Flags2;
                UnitFlagsDynamic = objPtr.DynamicFlags;

                EnergyPercentage = BotMath.Percentage(Energy, MaxEnergy);
                HealthPercentage = BotMath.Percentage(Health, MaxHealth);
                ManaPercentage = BotMath.Percentage(Mana, MaxMana);
                RagePercentage = BotMath.Percentage(Rage, MaxRage);
                RuneenergyPercentage = BotMath.Percentage(Runeenergy, MaxRuneenergy);
            }

            Auras = GetUnitAuras(xMemory, offsetList, BaseAddress, out int auraCount);
            AuraCount = auraCount;

            if (xMemory.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitPosition), out Vector3 position))
            {
                Position = position;
            }

            if (xMemory.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitRotation), out float rotation))
            {
                Rotation = rotation;
            }

            if (xMemory.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitIsAutoAttacking), out int isAutoAttacking))
            {
                IsAutoAttacking = isAutoAttacking == 1;
            }

            if (xMemory.Read(IntPtr.Add(BaseAddress, (int)offsetList.CurrentlyCastingSpellId), out int castingId))
            {
                CurrentlyCastingSpellId = castingId;
            }

            if (xMemory.Read(IntPtr.Add(BaseAddress, (int)offsetList.CurrentlyChannelingSpellId), out int channelingId))
            {
                CurrentlyChannelingSpellId = channelingId;
            }
        }

        public IEnumerable<RawWowAura> GetUnitAuras(XMemory xMemory, IOffsetList offsetList, IntPtr unitBase, out int auraCount)
        {
            if (xMemory.Read(IntPtr.Add(unitBase, (int)offsetList.AuraCount1), out int auraCount1))
            {
                if (auraCount1 == -1)
                {
                    if (xMemory.Read(IntPtr.Add(unitBase, (int)offsetList.AuraCount2), out int auraCount2)
                        && auraCount2 > 0
                        && xMemory.Read(IntPtr.Add(unitBase, (int)offsetList.AuraTable2), out IntPtr auraTable))
                    {
                        auraCount = auraCount2;
                        return ReadAuraTable(xMemory, auraTable, auraCount2);
                    }
                    else
                    {
                        auraCount = 0;
                    }
                }
                else
                {
                    auraCount = auraCount1;
                    return ReadAuraTable(xMemory, IntPtr.Add(unitBase, (int)offsetList.AuraTable1), auraCount1);
                }
            }
            else
            {
                auraCount = 0;
            }

            return Array.Empty<RawWowAura>();
        }

        private unsafe IEnumerable<RawWowAura> ReadAuraTable(XMemory xMemory, IntPtr buffBase, int auraCount)
        {
            List<RawWowAura> auras = new();

            if (auraCount > 40)
            {
                return auras;
            }

            for (int i = 0; i < auraCount; ++i)
            {
                xMemory.Read(buffBase + (sizeof(RawWowAura) * i), out RawWowAura rawWowAura);
                auras.Add(rawWowAura);
            }

            return auras;
        }
    }
}