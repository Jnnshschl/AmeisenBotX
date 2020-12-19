using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Core.Character.Spells.Objects;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PriestDiscipline : BasicCombatClass
    {
        public PriestDiscipline(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { powerWordFortitudeSpell, () => TryCastSpell(powerWordFortitudeSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { innerFireSpell, () => TryCastSpell(innerFireSpell, 0, true) }
            };

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

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 5) },
                { 4, new Talent(1, 4, 3) },
                { 5, new Talent(1, 5, 2) },
                { 7, new Talent(1, 7, 3) },
                { 8, new Talent(1, 8, 1) },
                { 9, new Talent(1, 9, 3) },
                { 11, new Talent(1, 11, 3) },
                { 14, new Talent(1, 14, 5) },
                { 15, new Talent(1, 15, 1) },
                { 16, new Talent(1, 16, 2) },
                { 17, new Talent(1, 17, 3) },
                { 18, new Talent(1, 18, 3) },
                { 19, new Talent(1, 19, 1) },
                { 20, new Talent(1, 20, 3) },
                { 21, new Talent(1, 21, 2) },
                { 22, new Talent(1, 22, 3) },
                { 23, new Talent(1, 23, 2) },
                { 24, new Talent(1, 24, 3) },
                { 25, new Talent(1, 25, 1) },
                { 26, new Talent(1, 26, 2) },
                { 27, new Talent(1, 27, 5) },
                { 28, new Talent(1, 28, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 4, new Talent(2, 4, 5) },
                { 6, new Talent(2, 6, 1) },
                { 8, new Talent(2, 8, 3) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Priest;

        private DateTime LastDeadPartymembersCheck { get; set; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void Execute()
        {
            base.Execute();

            if ((WowInterface.ObjectManager.PartymemberGuids.Any() || WowInterface.ObjectManager.Player.HealthPercentage < 75.0)
                && NeedToHealSomeone())
            {
                return;
            }

            if ((!WowInterface.ObjectManager.PartymemberGuids.Any() || WowInterface.ObjectManager.Player.ManaPercentage > 50) && SelectTarget(TargetManagerDps))
            {
                if (WowInterface.ObjectManager.Target.HasBuffByName(shadowWordPainSpell)
                    && TryCastSpell(shadowWordPainSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(smiteSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(holyShockSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }

                if (TryCastSpell(consecrationSpell, WowInterface.ObjectManager.TargetGuid, true))
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

        protected override Spell GetOpeningSpell()
        {
            throw new NotImplementedException();
        }

        private bool NeedToHealSomeone()
        {
            if (TargetManagerHeal.GetUnitToTarget(out IEnumerable<WowUnit> unitsToHeal))
            {
                WowUnit target = unitsToHeal.First();

                if (unitsToHeal.Count() > 3
                    && TryCastSpell(prayerOfHealingSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.Guid != WowInterface.ObjectManager.PlayerGuid
                    && target.HealthPercentage < 70
                    && WowInterface.ObjectManager.Player.HealthPercentage < 70
                    && TryCastSpell(bindingHealSpell, target.Guid, true))
                {
                    return true;
                }

                if (WowInterface.ObjectManager.Player.ManaPercentage < 50
                    && TryCastSpell(hymnOfHopeSpell, 0))
                {
                    return true;
                }

                if (WowInterface.ObjectManager.Player.HealthPercentage < 20
                    && TryCastSpell(desperatePrayerSpell, 0))
                {
                    return true;
                }

                if ((target.HealthPercentage < 98 && target.HealthPercentage > 80
                        && !target.HasBuffByName(weakenedSoulSpell)
                        && !target.HasBuffByName(powerWordShieldSpell)
                        && TryCastSpell(powerWordShieldSpell, target.Guid, true))
                    || (target.HealthPercentage < 90 && target.HealthPercentage > 80
                        && !target.HasBuffByName(renewSpell)
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