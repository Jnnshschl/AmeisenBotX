using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Raw;
using AmeisenBotX.Wow.Offsets;
using AmeisenBotX.Wow335a.Objects.Descriptors;
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

        public IEnumerable<RawWowAura> Auras { get; set; }

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

        public bool IsAutoAttacking { get; set; }

        public int Level => RawWowUnit.Level;

        public int Mana => RawWowUnit.Power1;

        public double ManaPercentage => BotMath.Percentage(Mana, MaxMana);

        public int MaxEnergy => RawWowUnit.MaxPower4;

        public int MaxHealth => RawWowUnit.MaxHealth;

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

        public static IEnumerable<RawWowAura> GetUnitAuras(IMemoryApi memoryApi, IOffsetList offsetList, IntPtr unitBase, out int auraCount)
        {
            if (memoryApi.Read(IntPtr.Add(unitBase, (int)offsetList.AuraCount1), out int auraCount1))
            {
                if (auraCount1 == -1)
                {
                    if (memoryApi.Read(IntPtr.Add(unitBase, (int)offsetList.AuraCount2), out int auraCount2)
                        && auraCount2 > 0
                        && memoryApi.Read(IntPtr.Add(unitBase, (int)offsetList.AuraTable2), out IntPtr auraTable))
                    {
                        auraCount = auraCount2;
                        return ReadAuraTable(memoryApi, auraTable, auraCount2);
                    }
                    else
                    {
                        auraCount = 0;
                    }
                }
                else
                {
                    auraCount = auraCount1;
                    return ReadAuraTable(memoryApi, IntPtr.Add(unitBase, (int)offsetList.AuraTable1), auraCount1);
                }
            }
            else
            {
                auraCount = 0;
            }

            return Array.Empty<RawWowAura>();
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

        public virtual string ReadName(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitName1), out IntPtr objName)
                && memoryApi.Read(IntPtr.Add(objName, (int)offsetList.WowUnitName2), out objName)
                && memoryApi.ReadString(objName, Encoding.UTF8, out string name))
            {
                return name;
            }

            return "unknown";
        }

        public override string ToString()
        {
            return $"Unit: {Guid} lvl. {Level} Position: {Position} DisplayId: {DisplayId}";
        }

        public override void Update(IMemoryApi memoryApi, IOffsetList offsetList)
        {
            base.Update(memoryApi, offsetList);

            if (memoryApi.Read(DescriptorAddress + WowObjectDescriptor335a.EndOffset, out WowUnitDescriptor335a objPtr))
            {
                RawWowUnit = objPtr;
            }

            Auras = GetUnitAuras(memoryApi, offsetList, BaseAddress, out int auraCount);
            AuraCount = auraCount;

            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitPosition), out Vector3 position))
            {
                Position = position;
            }

            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitPosition + 0x10), out float rotation))
            {
                Rotation = rotation;
            }

            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.WowUnitIsAutoAttacking), out int isAutoAttacking))
            {
                IsAutoAttacking = isAutoAttacking == 1;
            }

            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.CurrentlyCastingSpellId), out int castingId))
            {
                CurrentlyCastingSpellId = castingId;
            }

            if (memoryApi.Read(IntPtr.Add(BaseAddress, (int)offsetList.CurrentlyChannelingSpellId), out int channelingId))
            {
                CurrentlyChannelingSpellId = channelingId;
            }
        }

        private static unsafe IEnumerable<RawWowAura> ReadAuraTable(IMemoryApi memoryApi, IntPtr buffBase, int auraCount)
        {
            List<RawWowAura> auras = new();

            if (auraCount > 40)
            {
                return auras;
            }

            for (int i = 0; i < auraCount; ++i)
            {
                memoryApi.Read(buffBase + (sizeof(RawWowAura) * i), out RawWowAura rawWowAura);
                auras.Add(rawWowAura);
            }

            return auras;
        }
    }
}