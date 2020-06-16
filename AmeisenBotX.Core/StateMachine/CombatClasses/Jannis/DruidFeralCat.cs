using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class DruidFeralCat : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string barkskinSpell = "Barkskin";
        private const string berserkSpell = "Berserk";
        private const string catFormSpell = "Cat Form";
        private const string dashSpell = "Dash";
        private const string faerieFireSpell = "Faerie Fire (Feral)";
        private const string feralChargeSpell = "Feral Charge - Cat";
        private const string ferociousBiteSpell = "Ferocious Bite";
        private const string innervateSpell = "Innervate";
        private const string mangleSpell = "Mangle (Cat)";
        private const string markOfTheWildSpell = "Mark of the Wild";
        private const string rakeSpell = "Rake";
        private const string ripSpell = "Rip";
        private const string savageRoarSpell = "Savage Roar";
        private const string shredSpell = "Shred";
        private const string survivalInstinctsSpell = "Survival Instincts";
        private const string tigersFurySpell = "Tiger's Fury";
#pragma warning restore IDE0051

        public DruidFeralCat(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { markOfTheWildSpell, () => CastSpellIfPossible(markOfTheWildSpell, WowInterface.ObjectManager.PlayerGuid, true, 0, true) },
                { catFormSpell, () => CastSpellIfPossible(catFormSpell, 0, true) },
                { savageRoarSpell, () => CastSpellIfPossibleRogue(savageRoarSpell, WowInterface.ObjectManager.TargetGuid, true, true, 1) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { ripSpell, () => WowInterface.ObjectManager.Player.ComboPoints == 5 && CastSpellIfPossibleRogue(ripSpell, WowInterface.ObjectManager.TargetGuid, true, true, 5) },
                { rakeSpell, () => CastSpellIfPossible(rakeSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { mangleSpell, () => CastSpellIfPossible(mangleSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossible(faerieFireSpell, x.Guid, true) },
            };

            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Druid;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Feral (Cat) Druid spec.";

        public override string Displayname => "Druid Feral Cat";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicAgilityComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private TimegatedEvent AutoAttackEvent { get; set; }

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run() && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            double distanceToTarget = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position);

            if (distanceToTarget > 16
                && CastSpellIfPossible(dashSpell, 0))
            {
                return;
            }

            if (TargetAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (WowInterface.ObjectManager.Player.EnergyPercentage > 70
                    && CastSpellIfPossible(berserkSpell, 0))
                || (WowInterface.ObjectManager.Player.Energy < 30
                    && CastSpellIfPossible(tigersFurySpell, 0))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 70
                    && CastSpellIfPossible(barkskinSpell, 0, true))
                || (WowInterface.ObjectManager.Player.HealthPercentage < 35
                    && CastSpellIfPossible(survivalInstinctsSpell, 0, true))
                || (WowInterface.ObjectManager.Player.ComboPoints == 5
                    && CastSpellIfPossibleRogue(ferociousBiteSpell, WowInterface.ObjectManager.Target.Guid, true, true, 5))
                || CastSpellIfPossible(shredSpell, WowInterface.ObjectManager.Target.Guid, true))
            {
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