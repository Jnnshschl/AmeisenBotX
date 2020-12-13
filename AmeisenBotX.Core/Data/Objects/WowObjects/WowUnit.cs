using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObjects.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
{
    public class WowUnit : WowObject
    {
        private WowAura[] auras;

        public WowUnit(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public int AuraCount { get; set; }

        public IEnumerable<WowAura> Auras => auras;

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

        public bool IsWanding => CurrentlyCastingSpellId == 5019;

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

        public string Name { get; set; }

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

        public bool HasBuffById(int spellId)
        {
            return Auras != null && Auras.Any(e => e.SpellId == spellId);
        }

        public bool HasBuffByName(string name)
        {
            return Auras != null && Auras.Any(e => e.Name == name);
        }

        public bool IsInMeleeRange(WowUnit wowUnit) => wowUnit != null && Position.GetDistance(wowUnit.Position) < MathF.Max(4.5f, CombatReach + wowUnit.CombatReach + 1.0f);

        public override string ToString()
        {
            return $"Unit: [{Guid}] {Name} lvl. {Level}";
        }

        public unsafe override void Update()
        {
            base.Update();

            Name = string.Empty;

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowUnit objPtr))
            {
                Class = (WowClass)(objPtr.Class);
                CombatReach = objPtr.CombatReach;
                DisplayId = objPtr.DisplayId;
                Energy = objPtr.Power4;
                FactionTemplate = objPtr.FactionTemplate;
                Gender = (WowGender)(objPtr.Gender);
                Health = objPtr.Health;
                Level = objPtr.Level;
                Mana = objPtr.Power1;
                MaxEnergy = objPtr.MaxPower4;
                MaxHealth = objPtr.MaxHealth;
                MaxMana = objPtr.MaxPower1;
                MaxRage = objPtr.MaxPower2 / 10;
                MaxRuneenergy = objPtr.MaxPower7 / 10;
                NpcFlags = objPtr.NpcFlags;
                PowerType = (WowPowertype)(objPtr.PowerType);
                Race = (WowRace)(objPtr.Race);
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

            if (Type == WowObjectType.Unit)
            {
                // dont read the name for players
                Name = ReadUnitName();
            }

            auras = new WowAura[40];
            WowInterface.I.HookManager.WowGetUnitAuras(BaseAddress, ref auras, out int auraCount);
            AuraCount = auraCount;

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
        }

        private string ReadUnitName()
        {
            if (WowInterface.I.Db.TryGetUnitName(Guid, out string cachedName))
            {
                return cachedName;
            }

            if (WowInterface.I.XMemory.Read(IntPtr.Add(BaseAddress, (int)WowInterface.I.OffsetList.WowUnitName1), out IntPtr objName)
                && WowInterface.I.XMemory.Read(IntPtr.Add(objName, (int)WowInterface.I.OffsetList.WowUnitName2), out objName)
                && WowInterface.I.XMemory.ReadString(objName, Encoding.UTF8, out string name))
            {
                if (name.Length > 0)
                {
                    WowInterface.I.Db.CacheName(Guid, name);
                }

                return name;
            }

            return "unknown";
        }
    }
}