using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects.Flags;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Offsets;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AmeisenBotX.Wow.Objects
{
    public interface IWowUnit : IWowObject
    {
        int AuraCount { get; }

        IEnumerable<RawWowAura> Auras { get; }

        WowClass Class { get; }

        float CombatReach { get; }

        int CurrentlyCastingSpellId { get; }

        int CurrentlyChannelingSpellId { get; }

        int DisplayId { get; }

        int Energy { get; }

        double EnergyPercentage { get; }

        int FactionTemplate { get; }

        WowGender Gender { get; }

        int Health { get; }

        double HealthPercentage { get; }

        bool IsAuctioneer => NpcFlags[(int)WowUnitNpcFlags.Auctioneer];

        bool IsAutoAttacking { get; }

        bool IsBanker => NpcFlags[(int)WowUnitNpcFlags.Banker];

        bool IsBattlemaster => NpcFlags[(int)WowUnitNpcFlags.Battlemaster];

        bool IsCasting => CurrentlyCastingSpellId > 0 || CurrentlyChannelingSpellId > 0;

        bool IsClassTrainer => NpcFlags[(int)WowUnitNpcFlags.ClassTrainer];

        bool IsConfused => UnitFlags[(int)WowUnitFlags.Confused];

        bool IsDazed => UnitFlags[(int)WowUnitFlags.Dazed];

        bool IsDead => (Health == 0 || UnitFlagsDynamic[(int)WowUnitDynamicFlags.Dead]) && !UnitFlags2[(int)WowUnit2Flags.FeignDeath];

        bool IsDisarmed => UnitFlags[(int)WowUnitFlags.Disarmed];

        bool IsFleeing => UnitFlags[(int)WowUnitFlags.Fleeing];

        bool IsFlightMaster => NpcFlags[(int)WowUnitNpcFlags.FlightMaster];

        bool IsFoodVendor => NpcFlags[(int)WowUnitNpcFlags.FoodVendor];

        bool IsAmmoVendor => NpcFlags[(int)WowUnitNpcFlags.AmmoVendor];

        bool IsGossip => NpcFlags[(int)WowUnitNpcFlags.Gossip];

        bool IsGuard => NpcFlags[(int)WowUnitNpcFlags.Guard];

        bool IsGuildBanker => NpcFlags[(int)WowUnitNpcFlags.GuildBanker];

        bool IsInCombat => UnitFlags[(int)WowUnitFlags.Combat];

        bool IsInfluenced => UnitFlags[(int)WowUnitFlags.Influenced];

        bool IsInnkeeper => NpcFlags[(int)WowUnitNpcFlags.Innkeeper];

        bool IsInTaxiFlight => UnitFlags[(int)WowUnitFlags.TaxiFlight];

        bool IsLootable => UnitFlagsDynamic[(int)WowUnitDynamicFlags.Lootable];

        bool IsLooting => UnitFlags[(int)WowUnitFlags.Looting];

        bool IsMounted => UnitFlags[(int)WowUnitFlags.Mounted];

        bool IsNoneNpc => NpcFlags[(int)WowUnitNpcFlags.None];

        bool IsNotAttackable => UnitFlags[(int)WowUnitFlags.NotAttackable];

        bool IsNotSelectable => UnitFlags[(int)WowUnitFlags.NotSelectable];

        bool IsPetInCombat => UnitFlags[(int)WowUnitFlags.PetInCombat];

        bool IsPetition => NpcFlags[(int)WowUnitNpcFlags.Petitioner];

        bool IsPlayerControlled => UnitFlags[(int)WowUnitFlags.PlayerControlled];

        bool IsPlusMob => UnitFlags[(int)WowUnitFlags.PlusMob];

        bool IsPoisonVendor => NpcFlags[(int)WowUnitNpcFlags.PoisonVendor];

        bool IsPossessed => UnitFlags[(int)WowUnitFlags.Possessed];

        bool IsProfessionTrainer => NpcFlags[(int)WowUnitNpcFlags.ProfessionTrainer];

        bool IsPvpFlagged => UnitFlags[(int)WowUnitFlags.PvPFlagged];

        bool IsQuestgiver => NpcFlags[(int)WowUnitNpcFlags.Questgiver];

        bool IsReagentVendor => NpcFlags[(int)WowUnitNpcFlags.ReagentVendor];

        bool IsReferAFriendLinked => UnitFlagsDynamic[(int)WowUnitDynamicFlags.ReferAFriendLinked];

        bool IsRepairer => NpcFlags[(int)WowUnitNpcFlags.Repairer];

        bool IsSilenced => UnitFlags[(int)WowUnitFlags.Silenced];

        bool IsSitting => UnitFlags[(int)WowUnitFlags.Sitting];

        bool IsSkinnable => UnitFlags[(int)WowUnitFlags.Skinnable];

        bool IsSpecialInfo => UnitFlagsDynamic[(int)WowUnitDynamicFlags.SpecialInfo];

        bool IsSpellclick => NpcFlags[(int)WowUnitNpcFlags.Spellclick];

        bool IsSpiritGuide => NpcFlags[(int)WowUnitNpcFlags.SpiritGuide];

        bool IsSpiritHealer => NpcFlags[(int)WowUnitNpcFlags.SpiritHealer];

        bool IsStableMaster => NpcFlags[(int)WowUnitNpcFlags.StableMaster];

        bool IsTabardDesigner => NpcFlags[(int)WowUnitNpcFlags.TabardDesigner];

        bool IsTaggedByMe => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TaggedByMe];

        bool IsTaggedByOther => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TaggedByOther];

        bool IsTappedByAllThreatList => UnitFlagsDynamic[(int)WowUnitDynamicFlags.IsTappedByAllThreatList];

        bool IsTotem => UnitFlags[(int)WowUnitFlags.Totem];

        bool IsTrackedUnit => UnitFlagsDynamic[(int)WowUnitDynamicFlags.TrackUnit];

        bool IsTrainer => NpcFlags[(int)WowUnitNpcFlags.Trainer];

        bool IsVendor => NpcFlags[(int)WowUnitNpcFlags.Vendor];

        int Level { get; }

        int Mana { get; }

        double ManaPercentage { get; }

        int MaxEnergy { get; }

        int MaxHealth { get; }

        int MaxMana { get; }

        int MaxRage { get; }

        int MaxRuneenergy { get; }

        int MaxSecondary { get; }

        BitVector32 NpcFlags { get; }

        WowPowerType PowerType { get; }

        WowRace Race { get; }

        int Rage { get; }

        double RagePercentage { get; }

        float Rotation { get; }

        int Runeenergy { get; }

        double RuneenergyPercentage { get; }

        int Secondary { get; }

        double SecondaryPercentage { get; }

        ulong SummonedByGuid { get; }

        ulong TargetGuid { get; }

        BitVector32 UnitFlags { get; }

        BitVector32 UnitFlags2 { get; }

        BitVector32 UnitFlagsDynamic { get; }

        static bool IsValidUnit(IWowUnit unit)
        {
            return unit != null && !unit.IsNotAttackable;
        }

        bool HasBuffById(int spellId);

        bool IsInMeleeRange(IWowUnit wowUnit);

        string ReadName(IMemoryApi memoryApi, IOffsetList offsetList);
    }
}