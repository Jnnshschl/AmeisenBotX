using AmeisenBotX.Memory;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow.Objects.Flags;
using AmeisenBotX.Wow.Objects.Raw;
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

        bool IsAmmoVendor => NpcFlags[(int)WowUnitNpcFlag.AmmoVendor];

        bool IsAuctioneer => NpcFlags[(int)WowUnitNpcFlag.Auctioneer];

        bool IsAutoAttacking { get; }

        bool IsBanker => NpcFlags[(int)WowUnitNpcFlag.Banker];

        bool IsBattlemaster => NpcFlags[(int)WowUnitNpcFlag.Battlemaster];

        bool IsCasting => CurrentlyCastingSpellId > 0 || CurrentlyChannelingSpellId > 0;

        bool IsClassTrainer => NpcFlags[(int)WowUnitNpcFlag.ClassTrainer];

        bool IsConfused => UnitFlags[(int)WowUnitFlag.Confused];

        bool IsDazed => UnitFlags[(int)WowUnitFlag.Dazed];

        bool IsDead => (Health == 0 || UnitFlagsDynamic[(int)WowUnitDynamicFlag.Dead]) && !UnitFlags2[(int)WowUnit2Flag.FeignDeath];

        bool IsDisarmed => UnitFlags[(int)WowUnitFlag.Disarmed];

        bool IsFleeing => UnitFlags[(int)WowUnitFlag.Fleeing];

        bool IsFlightMaster => NpcFlags[(int)WowUnitNpcFlag.FlightMaster];

        bool IsFoodVendor => NpcFlags[(int)WowUnitNpcFlag.FoodVendor];

        bool IsGossip => NpcFlags[(int)WowUnitNpcFlag.Gossip];

        bool IsGuard => NpcFlags[(int)WowUnitNpcFlag.Guard];

        bool IsGuildBanker => NpcFlags[(int)WowUnitNpcFlag.GuildBanker];

        bool IsInCombat => UnitFlags[(int)WowUnitFlag.Combat];

        bool IsInfluenced => UnitFlags[(int)WowUnitFlag.Influenced];

        bool IsInnkeeper => NpcFlags[(int)WowUnitNpcFlag.Innkeeper];

        bool IsInTaxiFlight => UnitFlags[(int)WowUnitFlag.TaxiFlight];

        bool IsLootable => UnitFlagsDynamic[(int)WowUnitDynamicFlag.Lootable];

        bool IsLooting => UnitFlags[(int)WowUnitFlag.Looting];

        bool IsMounted => UnitFlags[(int)WowUnitFlag.Mounted];

        bool IsNoneNpc => NpcFlags[(int)WowUnitNpcFlag.None];

        bool IsNotAttackable => UnitFlags[(int)WowUnitFlag.NotAttackable];

        bool IsNotSelectable => UnitFlags[(int)WowUnitFlag.NotSelectable];

        bool IsPetInCombat => UnitFlags[(int)WowUnitFlag.PetInCombat];

        bool IsPetition => NpcFlags[(int)WowUnitNpcFlag.Petitioner];

        bool IsPlayerControlled => UnitFlags[(int)WowUnitFlag.PlayerControlled];

        bool IsPlusMob => UnitFlags[(int)WowUnitFlag.PlusMob];

        bool IsPoisonVendor => NpcFlags[(int)WowUnitNpcFlag.PoisonVendor];

        bool IsPossessed => UnitFlags[(int)WowUnitFlag.Possessed];

        bool IsProfessionTrainer => NpcFlags[(int)WowUnitNpcFlag.ProfessionTrainer];

        bool IsPvpFlagged => UnitFlags[(int)WowUnitFlag.PvPFlagged];

        bool IsQuestgiver => NpcFlags[(int)WowUnitNpcFlag.Questgiver];

        bool IsReagentVendor => NpcFlags[(int)WowUnitNpcFlag.ReagentVendor];

        bool IsReferAFriendLinked => UnitFlagsDynamic[(int)WowUnitDynamicFlag.ReferAFriendLinked];

        bool IsRepairer => NpcFlags[(int)WowUnitNpcFlag.Repairer];

        bool IsSilenced => UnitFlags[(int)WowUnitFlag.Silenced];

        bool IsSitting => UnitFlags[(int)WowUnitFlag.Sitting];

        bool IsSkinnable => UnitFlags[(int)WowUnitFlag.Skinnable];

        bool IsSpecialInfo => UnitFlagsDynamic[(int)WowUnitDynamicFlag.SpecialInfo];

        bool IsSpellclick => NpcFlags[(int)WowUnitNpcFlag.Spellclick];

        bool IsSpiritGuide => NpcFlags[(int)WowUnitNpcFlag.SpiritGuide];

        bool IsSpiritHealer => NpcFlags[(int)WowUnitNpcFlag.SpiritHealer];

        bool IsStableMaster => NpcFlags[(int)WowUnitNpcFlag.StableMaster];

        bool IsTabardDesigner => NpcFlags[(int)WowUnitNpcFlag.TabardDesigner];

        bool IsTaggedByMe => UnitFlagsDynamic[(int)WowUnitDynamicFlag.TaggedByMe];

        bool IsTaggedByOther => UnitFlagsDynamic[(int)WowUnitDynamicFlag.TaggedByOther];

        bool IsTappedByAllThreatList => UnitFlagsDynamic[(int)WowUnitDynamicFlag.IsTappedByAllThreatList];

        bool IsTotem => UnitFlags[(int)WowUnitFlag.Totem];

        bool IsTrackedUnit => UnitFlagsDynamic[(int)WowUnitDynamicFlag.TrackUnit];

        bool IsTrainer => NpcFlags[(int)WowUnitNpcFlag.Trainer];

        bool IsVendor => NpcFlags[(int)WowUnitNpcFlag.Vendor];

        int Level { get; }

        int Mana { get; }

        double ManaPercentage { get; }

        int MaxEnergy { get; }

        int MaxHealth { get; }

        int MaxMana { get; }

        int MaxRage { get; }

        int MaxRunicPower { get; }

        int MaxSecondary { get; }

        BitVector32 NpcFlags { get; }

        WowPowerType PowerType { get; }

        WowRace Race { get; }

        int Rage { get; }

        double RagePercentage { get; }

        int Resource => Class switch
        {
            WowClass.Deathknight => RunicPower,
            WowClass.Rogue => Energy,
            WowClass.Warrior => Rage,
            _ => Mana,
        };

        float Rotation { get; }

        int RunicPower { get; }

        double RunicPowerPercentage { get; }

        int Secondary { get; }

        double SecondaryPercentage { get; }

        ulong SummonedByGuid { get; }

        ulong TargetGuid { get; }

        BitVector32 UnitFlags { get; }

        BitVector32 UnitFlags2 { get; }

        BitVector32 UnitFlagsDynamic { get; }

        public new WowObjectType Type => WowObjectType.Unit;

        static bool IsValidUnit(IWowUnit unit)
        {
            return unit != null && !unit.IsNotAttackable;
        }

        float AggroRangeTo(IWowUnit other);

        bool HasBuffById(int spellId);

        bool IsInMeleeRange(IWowUnit wowUnit);

        float MeleeRangeTo(IWowUnit wowUnit);

        string ReadName(IMemoryApi memoryApi, IOffsetList offsetList);
    }
}