﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Memory;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Flags;
using AmeisenBotX.Wow335a.Objects.Descriptors;
using AmeisenBotX.Wow335a.Objects.Flags;
using AmeisenBotX.Wow335a.Objects.Raw;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Wow335a.Objects
{
    [Serializable]
    public class WowUnit335a : WowObject335a, IWowUnit
    {
        public int AuraCount { get; set; }

        public IEnumerable<IWowAura> Auras { get; set; }

        public WowClass Class => (WowClass)RawWowUnit.Class;

        public float CombatReach => RawWowUnit.CombatReach;

        public int CurrentlyCastingSpellId { get; set; }

        public int CurrentlyChannelingSpellId { get; set; }

        public int DisplayId => RawWowUnit.DisplayId;

        public int Energy => RawWowUnit.Power4;

        public double EnergyPercentage => BotMath.Percentage(Energy, MaxEnergy);

        public int FactionTemplate => RawWowUnit.FactionTemplate;

        public WowGender Gender => (WowGender)RawWowUnit.Gender;

        public int Health => RawWowUnit.Health;

        public double HealthPercentage => BotMath.Percentage(Health, MaxHealth);

        public int HolyPower => 0;

        public bool IsAutoAttacking { get; set; }

        public bool IsDead => (Health == 0 || UnitFlagsDynamic[(int)WowUnitDynamicFlags335a.Dead]) && !UnitFlags2[(int)WowUnit2Flag.FeignDeath];

        public bool IsLootable => UnitFlagsDynamic[(int)WowUnitDynamicFlags335a.Lootable];

        public bool IsReferAFriendLinked => UnitFlagsDynamic[(int)WowUnitDynamicFlags335a.ReferAFriendLinked];

        public bool IsSpecialInfo => UnitFlagsDynamic[(int)WowUnitDynamicFlags335a.SpecialInfo];

        public bool IsTaggedByMe => UnitFlagsDynamic[(int)WowUnitDynamicFlags335a.TaggedByMe];

        public bool IsTaggedByOther => UnitFlagsDynamic[(int)WowUnitDynamicFlags335a.TaggedByOther];

        public bool IsTappedByAllThreatList => UnitFlagsDynamic[(int)WowUnitDynamicFlags335a.IsTappedByAllThreatList];

        public bool IsTrackedUnit => UnitFlagsDynamic[(int)WowUnitDynamicFlags335a.TrackUnit];

        public int Level => RawWowUnit.Level;

        public int Mana => RawWowUnit.Power1;

        public double ManaPercentage => BotMath.Percentage(Mana, MaxMana);

        public int MaxEnergy => RawWowUnit.MaxPower4;

        public int MaxHealth => RawWowUnit.MaxHealth;

        public int MaxHolyPower => 0;

        public int MaxMana => RawWowUnit.MaxPower1;

        public int MaxRage => RawWowUnit.MaxPower2 / 10;

        public int MaxRunicPower => RawWowUnit.MaxPower7 / 10;

        public int MaxSecondary => Class switch
        {
            WowClass.Warrior => MaxRage,
            WowClass.Rogue => MaxEnergy,
            WowClass.Deathknight => MaxRunicPower,
            _ => MaxMana,
        };

        public BitVector32 NpcFlags => RawWowUnit.NpcFlags;

        public WowPowerType PowerType => (WowPowerType)RawWowUnit.PowerType;

        public WowRace Race => (WowRace)RawWowUnit.Race;

        public int Rage => RawWowUnit.Power2 / 10;

        public double RagePercentage => BotMath.Percentage(Rage, MaxRage);

        public float Rotation { get; set; }

        public int RunicPower => RawWowUnit.Power7 / 10;

        public double RunicPowerPercentage => BotMath.Percentage(RunicPower, MaxRunicPower);

        public int Secondary => Class switch
        {
            WowClass.Warrior => Rage,
            WowClass.Rogue => Energy,
            WowClass.Deathknight => RunicPower,
            _ => Mana,
        };

        public double SecondaryPercentage => Class switch
        {
            WowClass.Warrior => RagePercentage,
            WowClass.Rogue => EnergyPercentage,
            WowClass.Deathknight => RunicPowerPercentage,
            _ => ManaPercentage,
        };

        public ulong SummonedByGuid => RawWowUnit.SummonedBy;

        public ulong TargetGuid => RawWowUnit.Target;

        public BitVector32 UnitFlags => RawWowUnit.Flags1;

        public BitVector32 UnitFlags2 => RawWowUnit.Flags2;

        public BitVector32 UnitFlagsDynamic => RawWowUnit.DynamicFlags;

        protected WowUnitDescriptor335a RawWowUnit { get; private set; }

        public static IEnumerable<IWowAura> GetUnitAuras(IMemoryApi memory, IntPtr unitBase, out int auraCount)
        {
            if (memory.Read(IntPtr.Add(unitBase, (int)memory.Offsets.AuraCount1), out int auraCount1))
            {
                if (auraCount1 == -1)
                {
                    if (memory.Read(IntPtr.Add(unitBase, (int)memory.Offsets.AuraCount2), out int auraCount2)
                        && auraCount2 > 0
                        && memory.Read(IntPtr.Add(unitBase, (int)memory.Offsets.AuraTable2), out IntPtr auraTable))
                    {
                        auraCount = auraCount2;
                        return ReadAuraTable(memory, auraTable, auraCount2);
                    }
                    else
                    {
                        auraCount = 0;
                    }
                }
                else
                {
                    auraCount = auraCount1;
                    return ReadAuraTable(memory, IntPtr.Add(unitBase, (int)memory.Offsets.AuraTable1), auraCount1);
                }
            }
            else
            {
                auraCount = 0;
            }

            return Array.Empty<IWowAura>();
        }

        public float AggroRangeTo(IWowUnit other)
        {
            float range = 20.0f + (other.Level - Level);
            return MathF.Max(5.0f, MathF.Min(45.0f, range));
        }

        public bool HasBuffById(int spellId)
        {
            return Auras != null && Auras.Any(e => e.SpellId == spellId);
        }

        public bool IsInMeleeRange(IWowUnit wowUnit)
        {
            // TODO: figure out real way to use combat reach
            return wowUnit != null && Position.GetDistance(wowUnit.Position) < MeleeRangeTo(wowUnit);
        }

        public float MeleeRangeTo(IWowUnit wowUnit)
        {
            return wowUnit != null ? (wowUnit.CombatReach + CombatReach) * 0.95f : 0.0f;
        }

        public virtual string ReadName()
        {
            return GetDbEntry(Memory.Offsets.WowUnitDbEntryName, out IntPtr namePtr)
                && Memory.ReadString(namePtr, Encoding.UTF8, out string name) ? name : "unknown";
        }

        public virtual WowCreatureType ReadType()
        {
            return GetDbEntry(Memory.Offsets.WowUnitDbEntryType, out WowCreatureType type) ? type : WowCreatureType.Unknown;
        }

        public override string ToString()
        {
            return $"Unit: {Guid} lvl. {Level} Position: {Position} DisplayId: {DisplayId}";
        }

        public override void Update()
        {
            base.Update();

            if (Memory.Read(DescriptorAddress + WowObjectDescriptor335a.EndOffset, out WowUnitDescriptor335a objPtr))
            {
                RawWowUnit = objPtr;
            }

            Auras = GetUnitAuras(Memory, BaseAddress, out int auraCount);
            AuraCount = auraCount;

            if (Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowUnitPosition), out Vector3 position))
            {
                Position = position;
            }

            if (Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowUnitPosition + 0x10), out float rotation))
            {
                Rotation = rotation;
            }

            if (Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowUnitIsAutoAttacking), out int isAutoAttacking))
            {
                IsAutoAttacking = isAutoAttacking == 1;
            }

            if (Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.CurrentlyCastingSpellId), out int castingId))
            {
                CurrentlyCastingSpellId = castingId;
            }

            if (Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.CurrentlyChannelingSpellId), out int channelingId))
            {
                CurrentlyChannelingSpellId = channelingId;
            }
        }

        private static unsafe IEnumerable<IWowAura> ReadAuraTable(IMemoryApi memory, IntPtr buffBase, int auraCount)
        {
            List<IWowAura> auras = new();

            if (auraCount > 40)
            {
                return auras;
            }

            for (int i = 0; i < auraCount; ++i)
            {
                if (memory.Read(buffBase + (sizeof(WowAura335a) * i), out WowAura335a rawWowAura) && rawWowAura.SpellId > 0)
                {
                    auras.Add(rawWowAura);
                }
            }

            return auras;
        }

        private bool GetDbEntry<T>(IntPtr entryOffset, out T ptr) where T : unmanaged
        {
            ptr = default;
            return Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowUnitDbEntry), out IntPtr dbEntry)
                && dbEntry != IntPtr.Zero
                && Memory.Read(IntPtr.Add(dbEntry, (int)entryOffset), out ptr);
        }
    }
}