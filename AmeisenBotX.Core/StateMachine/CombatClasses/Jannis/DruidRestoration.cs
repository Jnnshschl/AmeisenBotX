using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DruidRestoration : BasicCombatClass
    {
        public DruidRestoration(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            UseDefaultTargetSelection = false;

            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { treeOfLifeSpell, () => CastSpellIfPossible(treeOfLifeSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { markOfTheWildSpell, () => CastSpellIfPossible(markOfTheWildSpell, WowInterface.ObjectManager.PlayerGuid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));

            SwiftmendEvent = new TimegatedEvent(TimeSpan.FromSeconds(15));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Druid;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Unholy Deathknight spec.";

        public override string Displayname => "Druid Restoration";

        public override bool HandlesMovement => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

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

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        private TimegatedEvent SwiftmendEvent { get; }

        public override void ExecuteCC()
        {
            if (WowInterface.ObjectManager.Player.ManaPercentage < 30
                && CastSpellIfPossible(innervateSpell, WowInterface.ObjectManager.PlayerGuid, true))
            {
                return;
            }

            if (!NeedToHealSomeone())
            {
                return;
            }
        }

        public override void OutOfCombatExecute()
        {
            if (GroupAuraManager.Tick()
                || MyAuraManager.Tick()
                || NeedToHealSomeone()
                || HandleDeadPartymembers(reviveSpell))
            {
                return;
            }
        }

        private bool NeedToHealSomeone()
        {
            if (TargetManager.GetUnitToTarget(out List<WowUnit> unitsToHeal)
                && unitsToHeal.Count > 0)
            {
                if (unitsToHeal.Count > 3
                    && CastSpellIfPossible(tranquilitySpell, 0, true))
                {
                    return true;
                }

                WowUnit target = unitsToHeal.First();

                if (target.HealthPercentage < 15
                    && CastSpellIfPossible(naturesSwiftnessSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 55
                    && !target.HasBuffByName(regrowthSpell)
                    && CastSpellIfPossible(regrowthSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 70
                    && (target.HasBuffByName(regrowthSpell) || target.HasBuffByName(rejuvenationSpell))
                    && SwiftmendEvent.Ready
                    && CastSpellIfPossible(swiftmendSpell, target.Guid, true)
                    && SwiftmendEvent.Run())
                {
                    return true;
                }

                if (target.HealthPercentage < 85
                        && !target.HasBuffByName(wildGrowthSpell)
                        && CastSpellIfPossible(wildGrowthSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 90
                        && !target.HasBuffByName(rejuvenationSpell)
                        && CastSpellIfPossible(rejuvenationSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 65
                    && (target.HasBuffByName(regrowthSpell) || target.HasBuffByName(rejuvenationSpell) || target.HasBuffByName(wildGrowthSpell))
                    && CastSpellIfPossible(nourishSpell, target.Guid, true))
                {
                    return true;
                }

                if (target.HealthPercentage < 85
                        && CastSpellIfPossible(healingTouchSpell, target.Guid, true))
                {
                    return true;
                }
            }

            return false;
        }
    }
}