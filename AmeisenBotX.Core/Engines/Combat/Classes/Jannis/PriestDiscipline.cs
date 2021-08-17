using AmeisenBotX.Core.Engines.Character.Comparators;
using AmeisenBotX.Core.Engines.Character.Talents.Objects;
using AmeisenBotX.Core.Logic.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis
{
    public class PriestDiscipline : BasicCombatClass
    {
        public PriestDiscipline(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, powerWordFortitudeSpell, () => TryCastSpell(powerWordFortitudeSpell, Bot.Wow.PlayerGuid, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, innerFireSpell, () => TryCastSpell(innerFireSpell, 0, true)));

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashHealSpell },
                { 400, flashHealSpell },
                { 3000, penanceSpell },
                { 5000, greaterHealSpell },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((powerWordFortitudeSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Discipline Priest spec.";

        public override string Displayname => "Priest Discipline";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Heal;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 2, new(1, 2, 5) },
                { 4, new(1, 4, 3) },
                { 5, new(1, 5, 2) },
                { 7, new(1, 7, 3) },
                { 8, new(1, 8, 1) },
                { 9, new(1, 9, 3) },
                { 11, new(1, 11, 3) },
                { 14, new(1, 14, 5) },
                { 15, new(1, 15, 1) },
                { 16, new(1, 16, 2) },
                { 17, new(1, 17, 3) },
                { 18, new(1, 18, 3) },
                { 19, new(1, 19, 1) },
                { 20, new(1, 20, 3) },
                { 21, new(1, 21, 2) },
                { 22, new(1, 22, 3) },
                { 23, new(1, 23, 2) },
                { 24, new(1, 24, 3) },
                { 25, new(1, 25, 1) },
                { 26, new(1, 26, 2) },
                { 27, new(1, 27, 5) },
                { 28, new(1, 28, 1) },
            },
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 4, new(2, 4, 5) },
                { 6, new(2, 6, 1) },
                { 8, new(2, 8, 3) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Priest;

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void Execute()
        {
            base.Execute();

            if ((Bot.Objects.PartymemberGuids.Any() || Bot.Player.HealthPercentage < 75.0)
                && NeedToHealSomeone())
            {
                return;
            }

            if ((!Bot.Objects.PartymemberGuids.Any() || Bot.Player.ManaPercentage > 50) && SelectTarget(TargetProviderDps))
            {
                if (Bot.Target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == shadowWordPainSpell)
                    && TryCastSpell(shadowWordPainSpell, Bot.Wow.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(smiteSpell, Bot.Wow.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(holyShockSpell, Bot.Wow.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(consecrationSpell, Bot.Wow.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealSomeone()
                || HandleDeadPartymembers(resurrectionSpell))
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetProviderHeal.Get(out IEnumerable<IWowUnit> unitsToHeal))
            {
                IWowUnit target = unitsToHeal.First();

                if (unitsToHeal.Count() > 3
                    && TryCastSpell(prayerOfHealingSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.Guid != Bot.Wow.PlayerGuid
                    && target.HealthPercentage < 70
                    && Bot.Player.HealthPercentage < 70
                    && TryCastSpell(bindingHealSpell, target.Guid, true))
                {
                    return true;
                }

                if (Bot.Player.ManaPercentage < 50
                    && TryCastSpell(hymnOfHopeSpell, 0))
                {
                    return true;
                }

                if (Bot.Player.HealthPercentage < 20
                    && TryCastSpell(desperatePrayerSpell, 0))
                {
                    return true;
                }

                if ((target.HealthPercentage < 98 && target.HealthPercentage > 80
                        && !target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == weakenedSoulSpell)
                        && !target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == powerWordShieldSpell)
                        && TryCastSpell(powerWordShieldSpell, target.Guid, true))
                    || (target.HealthPercentage < 90 && target.HealthPercentage > 80
                        && !target.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == renewSpell)
                        && TryCastSpell(renewSpell, target.Guid, true)))
                {
                    return true;
                }

                double healthDifference = target.MaxHealth - target.Health;
                List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                {
                    if (TryCastSpell(keyValuePair.Value, target.Guid, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}