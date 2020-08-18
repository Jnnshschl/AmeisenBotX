using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PriestHoly : BasicCombatClass
    {
        public PriestHoly(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { powerWordFortitudeSpell, () => TryCastSpell(powerWordFortitudeSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { innerFireSpell, () => TryCastSpell(innerFireSpell, 0, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, healSpell },
                { 100, flashHealSpell },
                { 5000, greaterHealSpell },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((powerWordFortitudeSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Holy Priest spec.";

        public override string Displayname => "Priest Holy";

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
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 2) },
                { 2, new Talent(2, 2, 3) },
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 8, new Talent(2, 8, 3) },
                { 9, new Talent(2, 9, 2) },
                { 10, new Talent(2, 10, 3) },
                { 12, new Talent(2, 12, 2) },
                { 13, new Talent(2, 13, 1) },
                { 14, new Talent(2, 14, 5) },
                { 15, new Talent(2, 15, 2) },
                { 16, new Talent(2, 16, 5) },
                { 17, new Talent(2, 17, 3) },
                { 22, new Talent(2, 22, 3) },
                { 23, new Talent(2, 23, 3) },
                { 24, new Talent(2, 24, 1) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 5) },
                { 27, new Talent(2, 27, 1) },
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

            if (WowInterface.ObjectManager.Player.ManaPercentage < 20
                && TryCastSpell(hymnOfHopeSpell, 0))
            {
                return;
            }

            if ((!WowInterface.ObjectManager.PartymemberGuids.Any() || WowInterface.ObjectManager.Player.ManaPercentage > 50) && SelectTarget(DpsTargetManager))
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
            if (HealTargetManager.GetUnitToTarget(out IEnumerable<WowUnit> unitsToHeal))
            {
                WowUnit target = unitsToHeal.First();

                if (unitsToHeal.Count() > 3
                    && target.HealthPercentage > 80.0
                    && TryCastSpell(prayerOfHealingSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 25.0
                    && TryCastSpell(guardianSpiritSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.Guid != WowInterface.ObjectManager.PlayerGuid
                    && target.HealthPercentage < 70.0
                    && WowInterface.ObjectManager.Player.HealthPercentage < 70.0
                    && TryCastSpell(bindingHealSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 90.0
                    && target.HealthPercentage > 75.0
                    && !target.HasBuffByName(renewSpell)
                    && TryCastSpell(renewSpell, target.Guid, true))
                {
                    return true;
                }

                double healthDifference = target.MaxHealth - target.Health;
                List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).OrderByDescending(e => e.Key).ToList();

                foreach (KeyValuePair<int, string> keyValuePair in spellsToTry)
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