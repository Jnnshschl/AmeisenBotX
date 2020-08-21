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
        public PaladinProtection(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { devotionAuraSpell, () => TryCastSpell(devotionAuraSpell, 0, true) },
                { blessingOfKingsSpell, () => TryCastSpell(blessingOfKingsSpell, 0, true) },
                { sealOfVengeanceSpell, () => TryCastSpell(sealOfVengeanceSpell, 0, true) },
                { righteousFurySpell, () => TryCastSpell(righteousFurySpell, 0, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpell(hammerOfJusticeSpell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((blessingOfKingsSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

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

        public override WowClass WowClass => WowClass.Paladin;

        private bool Use9SecSpell { get; set; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TankTargetManager))
            {
                if (WowInterface.ObjectManager.Player.HealthPercentage < 10.0
                    && TryCastSpell(layOnHandsSpell, 0, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Player.HealthPercentage < 20.0
                    && TryCastSpell(flashOfLightSpell, 0, true))
                {
                    return;
                }
                else if (WowInterface.ObjectManager.Player.HealthPercentage < 35.0
                    && TryCastSpell(holyLightSpell, 0, true))
                {
                    return;
                }

                if (TryCastSpell(sacredShieldSpell, 0, true)
                    || TryCastSpell(divinePleaSpell, 0, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target != null)
                {
                    if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                        && TryCastSpell(handOfReckoningSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        return;
                    }

                    if (TryCastSpell(avengersShieldSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || (WowInterface.ObjectManager.Target.HealthPercentage < 20.0 && TryCastSpell(hammerOfWrathSpell, WowInterface.ObjectManager.Target.Guid, true)))
                    {
                        return;
                    }

                    if (Use9SecSpell
                        && (((WowInterface.ObjectManager.Player.HasBuffByName(sealOfVengeanceSpell) || WowInterface.ObjectManager.Player.HasBuffByName(sealOfWisdomSpell))
                                && TryCastSpell(judgementOfLightSpell, WowInterface.ObjectManager.TargetGuid, true))
                            || TryCastSpell(consecrationSpell, WowInterface.ObjectManager.Target.Guid, true)
                            || TryCastSpell(holyShieldSpell, WowInterface.ObjectManager.Target.Guid, true)))
                    {
                        Use9SecSpell = false;
                        return;
                    }
                    else if (TryCastSpell(shieldOfTheRighteousnessSpell, WowInterface.ObjectManager.TargetGuid, true)
                             || TryCastSpell(hammerOfTheRighteousSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        Use9SecSpell = true;
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            Use9SecSpell = true;

            base.OutOfCombatExecute();
        }
    }
}