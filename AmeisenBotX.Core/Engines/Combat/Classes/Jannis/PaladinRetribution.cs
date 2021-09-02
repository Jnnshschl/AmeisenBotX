using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class PaladinRetribution : BasicCombatClass
    {
        public PaladinRetribution(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Paladin335a.BlessingOfMight, () => TryCastSpell(Paladin335a.BlessingOfMight, Bot.Wow.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Paladin335a.RetributionAura, () => TryCastSpell(Paladin335a.RetributionAura, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Paladin335a.SealOfVengeance, () => TryCastSpell(Paladin335a.SealOfVengeance, 0, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Paladin335a.HammerOfJustice, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((Paladin335a.BlessingOfMight, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Retribution Paladin spec.";

        public override string DisplayName => "Paladin Retribution";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Axe, WowWeaponType.Mace, WowWeaponType.Sword });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 5) },
                { 4, new(1, 4, 5) },
                { 6, new(1, 6, 1) },
            },
            Tree2 = new()
            {
                { 2, new(2, 2, 5) },
            },
            Tree3 = new()
            {
                { 2, new(3, 2, 5) },
                { 3, new(3, 3, 2) },
                { 4, new(3, 4, 3) },
                { 5, new(3, 5, 2) },
                { 7, new(3, 7, 5) },
                { 8, new(3, 8, 1) },
                { 9, new(3, 9, 2) },
                { 11, new(3, 11, 3) },
                { 12, new(3, 12, 3) },
                { 13, new(3, 13, 3) },
                { 14, new(3, 14, 1) },
                { 15, new(3, 15, 3) },
                { 17, new(3, 17, 2) },
                { 18, new(3, 18, 1) },
                { 19, new(3, 19, 3) },
                { 20, new(3, 20, 3) },
                { 21, new(3, 21, 2) },
                { 22, new(3, 22, 3) },
                { 23, new(3, 23, 1) },
                { 24, new(3, 24, 3) },
                { 25, new(3, 25, 3) },
                { 26, new(3, 26, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Paladin;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if ((Bot.Player.HealthPercentage < 20.0
                        && TryCastSpell(Paladin335a.LayOnHands, Bot.Wow.PlayerGuid))
                    || (Bot.Player.HealthPercentage < 60.0
                        && TryCastSpell(Paladin335a.HolyLight, Bot.Wow.PlayerGuid, true)))
                {
                    return;
                }

                if (((Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Paladin335a.SealOfVengeance) || Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Paladin335a.SealOfWisdom))
                        && TryCastSpell(Paladin335a.JudgementOfLight, Bot.Wow.TargetGuid, true))
                    || TryCastSpell(Paladin335a.AvengingWrath, 0, true)
                    || (Bot.Player.ManaPercentage < 80.0
                        && TryCastSpell(Paladin335a.DivinePlea, 0, true)))
                {
                    return;
                }

                if (Bot.Target != null)
                {
                    if ((Bot.Player.HealthPercentage < 20.0
                            && TryCastSpell(Paladin335a.HammerOfWrath, Bot.Wow.TargetGuid, true))
                        || TryCastSpell(Paladin335a.CrusaderStrike, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(Paladin335a.DivineStorm, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(Paladin335a.Consecration, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(Paladin335a.Exorcism, Bot.Wow.TargetGuid, true)
                        || TryCastSpell(Paladin335a.HolyWrath, Bot.Wow.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }
    }
}