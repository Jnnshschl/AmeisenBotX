using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PaladinProtection : BasicCombatClass
    {
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

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((blessingOfKingsSpell, (spellName, guid) => CastSpellIfPossible(spellName, guid, true)));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Paladin;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Protection Paladin spec.";

        public override string Displayname => "Paladin Protection";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicArmorComparator(null, new List<WeaponType>() { WeaponType.TWOHANDED_SWORDS, WeaponType.TWOHANDED_MACES, WeaponType.TWOHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Tank;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(2, 2, 5) },
                { 5, new Talent(2, 5, 5) },
                { 6, new Talent(2, 6, 1) },
                { 7, new Talent(2, 7, 3) },
                { 8, new Talent(2, 8, 5) },
                { 9, new Talent(2, 9, 2) },
                { 11, new Talent(2, 11, 3) },
                { 12, new Talent(2, 12, 1) },
                { 14, new Talent(2, 14, 2) },
                { 15, new Talent(2, 15, 3) },
                { 16, new Talent(2, 16, 1) },
                { 17, new Talent(2, 17, 1) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 3) },
                { 20, new Talent(2, 20, 3) },
                { 21, new Talent(2, 21, 3) },
                { 22, new Talent(2, 22, 1) },
                { 23, new Talent(2, 23, 2) },
                { 24, new Talent(2, 24, 3) },
                { 25, new Talent(2, 25, 2) },
                { 26, new Talent(2, 26, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 3, new Talent(3, 3, 2) },
                { 4, new Talent(3, 4, 3) },
                { 7, new Talent(3, 7, 5) },
                { 12, new Talent(3, 12, 3) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run() && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
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
                    || (WowInterface.ObjectManager.Target.HealthPercentage < 20.0 && CastSpellIfPossible(hammerOfWrathSpell, WowInterface.ObjectManager.Target.Guid, true))
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