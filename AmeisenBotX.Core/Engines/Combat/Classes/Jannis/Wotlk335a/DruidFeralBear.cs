using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis.Wotlk335a
{
    public class DruidFeralBear : BasicCombatClass
    {
        public DruidFeralBear(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.MarkOfTheWild, () => TryCastSpell(Druid335a.MarkOfTheWild, Bot.Wow.PlayerGuid, true, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.DireBearForm, () => TryCastSpell(Druid335a.DireBearForm, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Druid335a.MangleBear, () => TryCastSpell(Druid335a.MangleBear, Bot.Wow.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpell(Druid335a.Bash, x.Guid, true) },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((Druid335a.MarkOfTheWild, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Feral (Bear) Druid spec.";

        public override string DisplayName2 => "Druid Feral Bear";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicArmorComparator(new() { WowArmorType.Shield }, new() { WowWeaponType.Sword, WowWeaponType.Mace, WowWeaponType.Axe });

        public override WowRole Role => WowRole.Tank;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new(),
            Tree2 = new()
            {
                { 1, new(2, 1, 5) },
                { 3, new(2, 3, 1) },
                { 4, new(2, 4, 2) },
                { 5, new(2, 5, 3) },
                { 6, new(2, 6, 2) },
                { 7, new(2, 7, 1) },
                { 8, new(2, 8, 3) },
                { 10, new(2, 10, 3) },
                { 11, new(2, 11, 2) },
                { 12, new(2, 12, 2) },
                { 13, new(2, 13, 1) },
                { 14, new(2, 14, 1) },
                { 16, new(2, 16, 3) },
                { 17, new(2, 17, 5) },
                { 18, new(2, 18, 3) },
                { 19, new(2, 19, 1) },
                { 20, new(2, 20, 2) },
                { 22, new(2, 22, 3) },
                { 24, new(2, 24, 3) },
                { 25, new(2, 25, 3) },
                { 26, new(2, 26, 1) },
                { 27, new(2, 27, 3) },
                { 28, new(2, 28, 5) },
                { 29, new(2, 29, 1) },
                { 30, new(2, 30, 1) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 2) },
                { 3, new(3, 3, 3) },
                { 4, new(3, 4, 5) },
                { 8, new(3, 8, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Druid;

        public override WowVersion WowVersion => WowVersion.WotLK335a;

        public override void Execute()
        {
            base.Execute();

            if (TryFindTarget(TargetProviderDps, out _))
            {
                double distanceToTarget = Bot.Target.Position.GetDistance(Bot.Player.Position);

                if (distanceToTarget > 9.0
                    && TryCastSpell(Druid335a.FeralChargeBear, Bot.Wow.TargetGuid, true))
                {
                    return;
                }

                if (Bot.Player.HealthPercentage < 40
                    && TryCastSpell(Druid335a.SurvivalInstincts, 0, true))
                {
                    return;
                }

                if (Bot.Target.TargetGuid != Bot.Wow.PlayerGuid
                    && TryCastSpell(Druid335a.Growl, 0, true))
                {
                    return;
                }

                if (TryCastSpell(Druid335a.Berserk, 0))
                {
                    return;
                }

                if (NeedToHealMySelf())
                {
                    return;
                }

                int nearEnemies = Bot.GetNearEnemies<IWowUnit>(Bot.Player.Position, 10).Count();

                if ((Bot.Player.HealthPercentage > 80
                        && TryCastSpell(Druid335a.Enrage, 0, true))
                    || (Bot.Player.HealthPercentage < 70
                        && TryCastSpell(Druid335a.Barkskin, 0, true))
                    || (Bot.Player.HealthPercentage < 75
                        && TryCastSpell(Druid335a.FrenziedRegeneration, 0, true))
                    || (nearEnemies > 2 && TryCastSpell(Druid335a.ChallengingRoar, 0, true))
                    || TryCastSpell(Druid335a.Lacerate, Bot.Wow.TargetGuid, true)
                    || (nearEnemies > 2 && TryCastSpell(Druid335a.Swipe, 0, true))
                    || TryCastSpell(Druid335a.MangleBear, Bot.Wow.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealMySelf())
            {
                return;
            }
        }

        private bool NeedToHealMySelf()
        {
            if (Bot.Player.HealthPercentage < 60
                && !Bot.Player.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == Druid335a.Rejuvenation)
                && TryCastSpell(Druid335a.Rejuvenation, 0, true))
            {
                return true;
            }

            if (Bot.Player.HealthPercentage < 40
                && TryCastSpell(Druid335a.HealingTouch, 0, true))
            {
                return true;
            }

            return false;
        }
    }
}