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
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DruidFeralBear : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string barkskinSpell = "Barkskin";
        private const string bashSpell = "Bash";
        private const string berserkSpell = "Berserk";
        private const string challengingRoarSpell = "Challenging Roar";
        private const string direBearFormSpell = "Dire Bear Form";
        private const string enrageSpell = "Enrage";
        private const string faerieFireSpell = "Faerie Fire (Feral)";
        private const string feralChargeSpell = "Feral Charge - Bear";
        private const string frenziedRegenerationSpell = "Frenzied Regeneration";
        private const string growlSpell = "Growl";
        private const string healingTouchSpell = "Healing Touch";
        private const string innervateSpell = "Innervate";
        private const string lacerateSpell = "Lacerate";
        private const string mangleSpell = "Mangle (Bear)";
        private const string markOfTheWildSpell = "Mark of the Wild";
        private const string rejuvenationSpell = "Rejuvenation";
        private const string survivalInstinctsSpell = "Survival Instincts";
        private const string swipeSpell = "Swipe (Bear)";
#pragma warning restore IDE0051

        public DruidFeralBear(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { markOfTheWildSpell, () => CastSpellIfPossible(markOfTheWildSpell, WowInterface.ObjectManager.PlayerGuid, true, 0, true) },
                { direBearFormSpell, () => CastSpellIfPossible(direBearFormSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { faerieFireSpell, () => CastSpellIfPossible(faerieFireSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { mangleSpell, () => CastSpellIfPossible(mangleSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossible(bashSpell, x.Guid, true) },
            };

            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Druid;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Feral (Bear) Druid spec.";

        public override string Displayname => "Druid Feral Bear";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicArmorComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Tank;

        public override string Version => "1.0";

        private bool InHealCombo { get; set; }

        private TimegatedEvent AutoAttackEvent { get; set; }

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 5) },
                { 3, new Talent(2, 3, 1) },
                { 4, new Talent(2, 4, 2) },
                { 5, new Talent(2, 5, 3) },
                { 6, new Talent(2, 6, 2) },
                { 7, new Talent(2, 7, 1) },
                { 8, new Talent(2, 8, 3) },
                { 10, new Talent(2, 10, 3) },
                { 11, new Talent(2, 11, 2) },
                { 12, new Talent(2, 12, 2) },
                { 13, new Talent(2, 13, 1) },
                { 14, new Talent(2, 14, 1) },
                { 16, new Talent(2, 16, 3) },
                { 17, new Talent(2, 17, 5) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 1) },
                { 20, new Talent(2, 20, 2) },
                { 22, new Talent(2, 22, 3) },
                { 24, new Talent(2, 24, 3) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 1) },
                { 27, new Talent(2, 27, 3) },
                { 28, new Talent(2, 28, 5) },
                { 29, new Talent(2, 29, 1) },
                { 30, new Talent(2, 30, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 2) },
                { 3, new Talent(3, 3, 3) },
                { 4, new Talent(3, 4, 5) },
                { 8, new Talent(3, 8, 1) },
            },
        };

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run() && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                && CastSpellIfPossible(growlSpell, 0, true))
            {
                return;
            }

            int nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 10).Count();

            if (TargetAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (WowInterface.ObjectManager.Player.HealthPercentage < 70
                    && CastSpellIfPossible(barkskinSpell, 0, true))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 50
                    && WowInterface.ObjectManager.Player.RagePercentage > 50
                    && CastSpellIfPossible(frenziedRegenerationSpell, 0, true))
                || (WowInterface.ObjectManager.Player.HealthPercentage > 80
                    && WowInterface.ObjectManager.Player.RagePercentage < 10
                    && CastSpellIfPossible(enrageSpell, 0, true))
                || (nearEnemies > 2 && CastSpellIfPossible(challengingRoarSpell, 0, true))
                || CastSpellIfPossible(feralChargeSpell, WowInterface.ObjectManager.TargetGuid, true)
                || CastSpellIfPossible(lacerateSpell, WowInterface.ObjectManager.TargetGuid, true)
                || (nearEnemies > 2 && CastSpellIfPossible(swipeSpell, 0, true)))
            {
                return;
            }

            if (InHealCombo
                && WowInterface.ObjectManager.Player.HealthPercentage < 25
                && CastSpellIfPossible(rejuvenationSpell, 0, true))
            {
                InHealCombo = false;
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 25
                && CastSpellIfPossible(healingTouchSpell, 0, true))
            {
                InHealCombo = WowInterface.ObjectManager.Player.ManaPercentage > 15;
                return;
            }
        }

        public override void OutOfCombatExecute()
        {
            if (GroupAuraManager.Tick()
                || MyAuraManager.Tick())
            {
                return;
            }
        }
    }
}