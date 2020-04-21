using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
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

        public DruidFeralBear(WowInterface wowInterface) : base(wowInterface)
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
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Druid;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Feral (Bear) Druid spec.";

        public override string Displayname => "Druid Feral Bear";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicArmorComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Tank;

        public override string Version => "1.0";

        private bool InHealCombo { get; set; }

        private DateTime LastAutoAttackCheck { get; set; }

        public override void ExecuteCC()
        {
            if (DateTime.Now - LastAutoAttackCheck > TimeSpan.FromSeconds(4) && !WowInterface.ObjectManager.Player.IsAutoAttacking)
            {
                LastAutoAttackCheck = DateTime.Now;
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                && CastSpellIfPossible(growlSpell, 0, true))
            {
                return;
            }

            int nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 10).Count();

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
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
            MyAuraManager.Tick();
        }
    }
}