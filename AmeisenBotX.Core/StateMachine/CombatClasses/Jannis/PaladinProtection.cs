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
    public class PaladinProtection : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string avengersShieldSpell = "Avenger\'s Shield";
        private const string blessingOfKingsSpell = "Blessing of Kings";
        private const string consecrationSpell = "Consecration";
        private const string devotionAuraSpell = "Devotion Aura";
        private const string divinePleaSpell = "Divine Plea";
        private const string flashOfLightSpell = "Flash of Light";
        private const string hammerOfJusticeSpell = "Hammer of Justice";
        private const string hammerOfTheRighteousSpell = "Hammer of the Righteous";
        private const string hammerOfWrathSpell = "Hammer of Wrath";
        private const string holyLightSpell = "Holy Light";
        private const string holyShieldSpell = "Holy Shield";
        private const string judgementOfLightSpell = "Judgement of Light";
        private const string layOnHandsSpell = "Lay on Hands";
        private const string righteousFurySpell = "Righteous Fury";
        private const string sealOfVengeanceSpell = "Seal of Vengeance";
        private const string shieldOfTheRighteousnessSpell = "Shield of the Righteousness";
#pragma warning restore IDE0051

        public PaladinProtection(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { devotionAuraSpell, () => CastSpellIfPossible(devotionAuraSpell, 0, true) },
                { blessingOfKingsSpell, () => CastSpellIfPossible(blessingOfKingsSpell, 0, true) },
                { sealOfVengeanceSpell, () => CastSpellIfPossible(sealOfVengeanceSpell, 0, true) },
                { righteousFurySpell, () => CastSpellIfPossible(righteousFurySpell, 0, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossible(hammerOfJusticeSpell, x.Guid, true) }
            };

            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(4000));

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((blessingOfKingsSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Paladin;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Protection Paladin spec.";

        public override string Displayname => "Paladin Protection";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => true;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicArmorComparator(null, new List<WeaponType>() { WeaponType.TWOHANDED_SWORDS, WeaponType.TWOHANDED_MACES, WeaponType.TWOHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Tank;

        public override string Version => "1.0";

        private TimegatedEvent AutoAttackEvent { get; set; }

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (TargetAuraManager.Tick()
                || TargetInterruptManager.Tick())
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 10
                && CastSpellIfPossible(layOnHandsSpell, 0, true))
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 20
                && CastSpellIfPossible(flashOfLightSpell, 0, true))
            {
                return;
            }
            else if (WowInterface.ObjectManager.Player.HealthPercentage < 35
                && CastSpellIfPossible(holyLightSpell, 0, true))
            {
                return;
            }

            if (WowInterface.ObjectManager.Player.ManaPercentage < 25
                && CastSpellIfPossible(divinePleaSpell, 0, true))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if (CastSpellIfPossible(avengersShieldSpell, WowInterface.ObjectManager.Target.Guid, true)
                    || CastSpellIfPossible(hammerOfWrathSpell, WowInterface.ObjectManager.Target.Guid, true)
                    || CastSpellIfPossible(judgementOfLightSpell, WowInterface.ObjectManager.Target.Guid, true)
                    || CastSpellIfPossible(hammerOfTheRighteousSpell, WowInterface.ObjectManager.Target.Guid, true)
                    || CastSpellIfPossible(consecrationSpell, WowInterface.ObjectManager.Target.Guid, true)
                    || CastSpellIfPossible(shieldOfTheRighteousnessSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || CastSpellIfPossible(holyShieldSpell, WowInterface.ObjectManager.Target.Guid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || GroupAuraManager.Tick())
            {
                return;
            }
        }
    }
}