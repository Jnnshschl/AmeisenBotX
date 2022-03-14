using AmeisenBotX.Common.Math;
using AmeisenBotX.Memory;
using AmeisenBotX.Wow;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Flags;
using AmeisenBotX.Wow335a.Objects.Raw;
using AmeisenBotX.Wow548.Objects.Descriptors;
using AmeisenBotX.Wow548.Objects.Flags;
using System.Collections.Specialized;
using System.Text;

namespace AmeisenBotX.Wow548.Objects
{
    [Serializable]
    public unsafe class WowUnit548 : WowObject548, IWowUnit
    {
        private IEnumerable<IWowAura>? AuraTable;

        private WowUnitDescriptor548? UnitDescriptor;

        public int AuraCount => ReadAuraTable().Count();

        public IEnumerable<IWowAura> Auras => ReadAuraTable();

        public WowClass Class => (WowClass)GetUnitDescriptor().Class;

        public float CombatReach => GetUnitDescriptor().CombatReach;

        public int CurrentlyCastingSpellId => Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.CurrentlyCastingSpellId), out int castingId) ? castingId : 0;

        public int CurrentlyChannelingSpellId => Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.CurrentlyChannelingSpellId), out int channelingId) ? channelingId : 0;

        public int DisplayId => GetUnitDescriptor().DisplayId;

        public int Energy => GetUnitDescriptor().Power4;

        public double EnergyPercentage => BotMath.Percentage(Energy, MaxEnergy);

        public int FactionTemplate => GetUnitDescriptor().FactionTemplate;

        public WowGender Gender => (WowGender)GetUnitDescriptor().Gender;

        public int Health => GetUnitDescriptor().Health;

        public double HealthPercentage => BotMath.Percentage(Health, MaxHealth);

        public int HolyPower => GetUnitDescriptor().Power2;

        public bool IsAutoAttacking => Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowUnitIsAutoAttacking), out int isAutoAttacking) && isAutoAttacking == 1;

        public bool IsDead => (Health == 0 || UnitFlagsDynamic[(int)WowUnitDynamicFlags548.Dead]) && !UnitFlags2[(int)WowUnit2Flag.FeignDeath];

        public bool IsLootable => UnitFlagsDynamic[(int)WowUnitDynamicFlags548.Lootable];

        public bool IsReferAFriendLinked => UnitFlagsDynamic[(int)WowUnitDynamicFlags548.ReferAFriendLinked];

        public bool IsSpecialInfo => UnitFlagsDynamic[(int)WowUnitDynamicFlags548.SpecialInfo];

        public bool IsTaggedByMe => UnitFlagsDynamic[(int)WowUnitDynamicFlags548.TaggedByMe];

        public bool IsTaggedByOther => UnitFlagsDynamic[(int)WowUnitDynamicFlags548.TaggedByOther];

        public bool IsTappedByAllThreatList => UnitFlagsDynamic[(int)WowUnitDynamicFlags548.IsTappedByAllThreatList];

        public bool IsTrackedUnit => UnitFlagsDynamic[(int)WowUnitDynamicFlags548.TrackUnit];

        public int Level => GetUnitDescriptor().Level;

        public int Mana => GetUnitDescriptor().Power1;

        public double ManaPercentage => BotMath.Percentage(Mana, MaxMana);

        public int MaxEnergy => GetUnitDescriptor().MaxPower4;

        public int MaxHealth => GetUnitDescriptor().MaxHealth;

        public int MaxHolyPower => GetUnitDescriptor().MaxPower2;

        public int MaxMana => GetUnitDescriptor().MaxPower1;

        public int MaxRage => GetUnitDescriptor().MaxPower1 / 10;

        public int MaxRunicPower => 0;

        public int MaxSecondary => Class switch
        {
            WowClass.Warrior => MaxRage,
            WowClass.Rogue => MaxEnergy,
            WowClass.Deathknight => MaxRunicPower,
            _ => MaxMana,
        };

        public BitVector32 NpcFlags => GetUnitDescriptor().NpcFlags1;

        public new Vector3 Position => Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowUnitPosition), out Vector3 position) ? position : Vector3.Zero;

        public WowPowerType PowerType => (WowPowerType)GetUnitDescriptor().PowerType;

        public WowRace Race => (WowRace)GetUnitDescriptor().Race;

        public int Rage => GetUnitDescriptor().Power1 / 10;

        public double RagePercentage => BotMath.Percentage(Rage, MaxRage);

        public float Rotation => Memory.Read(IntPtr.Add(BaseAddress, (int)Memory.Offsets.WowUnitPosition + 0x10), out float rotation) ? rotation : 0.0f;

        public int RunicPower => 0;

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

        public ulong SummonedByGuid => GetUnitDescriptor().SummonedBy;

        public ulong TargetGuid => GetUnitDescriptor().Target;

        public BitVector32 UnitFlags => GetUnitDescriptor().Flags1;

        public BitVector32 UnitFlags2 => GetUnitDescriptor().Flags2;

        public static IEnumerable<IWowAura> GetUnitAuras(WowMemoryApi memory, IntPtr unitBase, out int auraCount)
        {
            if (memory.Read(IntPtr.Add(unitBase, (int)memory.Offsets.AuraCount1), out int auraCount1))
            {
                if (auraCount1 == -1)
                {
                    if (memory.Read(IntPtr.Add(unitBase, (int)memory.Offsets.AuraCount2), out int auraCount2)
                        && auraCount2 > 0
                        && memory.Read(IntPtr.Add(unitBase, (int)memory.Offsets.AuraTable2), out IntPtr auraTable))
                    {
                        IEnumerable<IWowAura> auras = ReadAuraTable(memory, auraTable, auraCount2);
                        auraCount = auras.Count();
                        return auras;
                    }
                }
                else
                {
                    IEnumerable<IWowAura> auras = ReadAuraTable(memory, IntPtr.Add(unitBase, (int)memory.Offsets.AuraTable1), auraCount1);
                    auraCount = auras.Count();
                    return auras;
                }
            }

            auraCount = 0;
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

        public virtual string ReadName(WowMemoryApi memory)
        {
            if (memory.Read(IntPtr.Add(BaseAddress, (int)memory.Offsets.WowUnitName1), out IntPtr objName)
                && memory.Read(IntPtr.Add(objName, (int)memory.Offsets.WowUnitName2), out objName)
                && memory.ReadString(objName, Encoding.UTF8, out string name))
            {
                return name;
            }

            return "unknown";
        }

        public override string ToString()
        {
            return $"Unit: {Guid} lvl. {Level} Position: {Position} DisplayId: {DisplayId}";
        }

        public override void Update(WowMemoryApi memory)
        {
            base.Update(memory);
        }

        protected WowUnitDescriptor548 GetUnitDescriptor()
        {
            return UnitDescriptor ??= Memory.Read(DescriptorAddress + sizeof(WowObjectDescriptor548), out WowUnitDescriptor548 objPtr) ? objPtr : new();
        }

        private static unsafe IEnumerable<IWowAura> ReadAuraTable(IMemoryApi memory, IntPtr buffBase, int auraCount)
        {
            List<IWowAura> auras = new();

            for (int i = 0; i < auraCount; ++i)
            {
                if (memory.Read(buffBase + (sizeof(WowAura548) * i), out WowAura548 rawWowAura) && rawWowAura.SpellId > 0)
                {
                    auras.Add(rawWowAura);
                }
            }

            return auras;
        }

        private IEnumerable<IWowAura> ReadAuraTable()
        {
            return AuraTable ??= GetUnitAuras(Memory, BaseAddress, out _);
        }
    }
}