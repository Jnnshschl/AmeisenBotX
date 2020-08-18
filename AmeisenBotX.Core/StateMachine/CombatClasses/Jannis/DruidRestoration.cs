using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Enums;
using AmeisenBotX.Core.Statemachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DruidRestoration : BasicCombatClass
    {
        public DruidRestoration(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { treeOfLifeSpell, () => WowInterface.ObjectManager.PartymemberGuids.Any() && TryCastSpell(treeOfLifeSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { markOfTheWildSpell, () => TryCastSpell(markOfTheWildSpell, WowInterface.ObjectManager.PlayerGuid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));

            SwiftmendEvent = new TimegatedEvent(TimeSpan.FromSeconds(15));
        }

        public override string Description => "FCFS based CombatClass for the Druid Restoration spec.";

        public override string Displayname => "Druid Restoration";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicComparator
        (
            new List<ArmorType>() { ArmorType.SHIELDS },
            new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES },
            new Dictionary<string, double>()
            {
                { "ITEM_MOD_CRIT_RATING_SHORT", 1.2 },
                { "ITEM_MOD_INTELLECT_SHORT", 1.0 },
                { "ITEM_MOD_SPELL_POWER_SHORT", 1.6 },
                { "ITEM_MOD_HASTE_RATING_SHORT", 1.8 },
                { "ITEM_MOD_SPIRIT_SHORT ", 1.4 },
                { "ITEM_MOD_POWER_REGEN0_SHORT", 1.4 },
            }
        );

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 5) },
                { 3, new Talent(1, 3, 3) },
                { 4, new Talent(1, 4, 2) },
                { 8, new Talent(1, 8, 1) },
            },
            Tree2 = new Dictionary<int, Talent>(),
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 2) },
                { 2, new Talent(3, 2, 3) },
                { 5, new Talent(3, 5, 3) },
                { 6, new Talent(3, 6, 3) },
                { 7, new Talent(3, 7, 3) },
                { 8, new Talent(3, 8, 1) },
                { 9, new Talent(3, 9, 2) },
                { 11, new Talent(3, 11, 3) },
                { 12, new Talent(3, 12, 1) },
                { 13, new Talent(3, 13, 5) },
                { 14, new Talent(3, 14, 2) },
                { 16, new Talent(3, 16, 5) },
                { 17, new Talent(3, 17, 3) },
                { 18, new Talent(3, 18, 1) },
                { 20, new Talent(3, 20, 5) },
                { 21, new Talent(3, 21, 3) },
                { 22, new Talent(3, 22, 3) },
                { 23, new Talent(3, 23, 1) },
                { 24, new Talent(3, 24, 3) },
                { 25, new Talent(3, 25, 2) },
                { 26, new Talent(3, 26, 5) },
                { 27, new Talent(3, 27, 1) },
            },
        };

        public override bool UseAutoAttacks => false;

        public override string Version => "1.1";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Druid;

        private TimegatedEvent SwiftmendEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (WowInterface.ObjectManager.Player.ManaPercentage < 30.0
                && TryCastSpell(innervateSpell, 0, true))
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 50.0
                && TryCastSpell(barkskinSpell, 0, true))
            {
                return;
            }

            if (WowInterface.ObjectManager.Partymembers.Any(e => !e.IsDead))
            {
                if (NeedToHealSomeone())
                {
                    return;
                }
            }
            else
            {
                // when we're solo, we don't need to heal as much as we would do in a dungeon group
                if (WowInterface.ObjectManager.Player.HealthPercentage < 75.0 && NeedToHealSomeone())
                {
                    return;
                }

                if (SelectTarget(DpsTargetManager))
                {
                    if (!WowInterface.ObjectManager.Target.HasBuffByName(moonfireSpell)
                        && TryCastSpell(moonfireSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }

                    if (TryCastSpell(starfireSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }

                    if (TryCastSpell(wrathSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealSomeone()
                || HandleDeadPartymembers(reviveSpell))
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (HealTargetManager.GetUnitToTarget(out IEnumerable<WowUnit> unitsToHeal))
            {
                if (unitsToHeal.Count(e => e.HealthPercentage < 40.0) > 3
                    && TryCastSpell(tranquilitySpell, 0, true))
                {
                    return true;
                }

                WowUnit target = unitsToHeal.First();

                if (target.HealthPercentage < 90.0
                    && target.HealthPercentage > 78.0
                    && unitsToHeal.Count(e => e.HealthPercentage < 75.0) > 2
                    && TryCastSpell(wildGrowthSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 20.0
                    && TryCastSpell(naturesSwiftnessSpell, target.Guid, true)
                    && TryCastSpell(healingTouchSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 50.0
                    && (target.HasBuffByName(regrowthSpell) || target.HasBuffByName(rejuvenationSpell))
                    && SwiftmendEvent.Ready
                    && TryCastSpell(swiftmendSpell, target.Guid, true)
                    && SwiftmendEvent.Run())
                {
                    return true;
                }

                if (target.HealthPercentage < 95.0
                    && target.HealthPercentage > 80.0
                    && !target.HasBuffByName(rejuvenationSpell)
                    && TryCastSpell(rejuvenationSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 98.0
                    && target.HealthPercentage > 80.0
                    && !target.HasBuffByName(lifebloomSpell)
                    && TryCastSpell(lifebloomSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 85.0
                    && TryCastSpell(regrowthSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 85.0
                    && TryCastSpell(nourishSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 85.0
                    && TryCastSpell(healingTouchSpell, target.Guid, true))
                {
                    return true;
                }
            }

            return false;
        }
    }
}