using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using AmeisenBotX.Core.Character.Spells.Objects;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class PaladinRetribution : BasicCombatClass
    {
        public PaladinRetribution(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { blessingOfMightSpell, () => TryCastSpell(blessingOfMightSpell, WowInterface.ObjectManager.PlayerGuid, true) },
                { retributionAuraSpell, () => TryCastSpell(retributionAuraSpell, 0, true) },
                { sealOfVengeanceSpell, () => TryCastSpell(sealOfVengeanceSpell, 0, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpell(hammerOfJusticeSpell, x.Guid, true) }
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((blessingOfMightSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Retribution Paladin spec.";

        public override string Displayname => "Paladin Retribution";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_AXES, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_SWORDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(1, 2, 5) },
                { 4, new Talent(1, 4, 5) },
                { 6, new Talent(1, 6, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(2, 2, 5) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(3, 2, 5) },
                { 3, new Talent(3, 3, 2) },
                { 4, new Talent(3, 4, 3) },
                { 5, new Talent(3, 5, 2) },
                { 7, new Talent(3, 7, 5) },
                { 8, new Talent(3, 8, 1) },
                { 9, new Talent(3, 9, 2) },
                { 11, new Talent(3, 11, 3) },
                { 12, new Talent(3, 12, 3) },
                { 13, new Talent(3, 13, 3) },
                { 14, new Talent(3, 14, 1) },
                { 15, new Talent(3, 15, 3) },
                { 17, new Talent(3, 17, 2) },
                { 18, new Talent(3, 18, 1) },
                { 19, new Talent(3, 19, 3) },
                { 20, new Talent(3, 20, 3) },
                { 21, new Talent(3, 21, 2) },
                { 22, new Talent(3, 22, 3) },
                { 23, new Talent(3, 23, 1) },
                { 24, new Talent(3, 24, 3) },
                { 25, new Talent(3, 25, 3) },
                { 26, new Talent(3, 26, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Paladin;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
                if ((WowInterface.ObjectManager.Player.HealthPercentage < 20.0
                        && TryCastSpell(layOnHandsSpell, WowInterface.ObjectManager.PlayerGuid))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 60.0
                        && TryCastSpell(holyLightSpell, WowInterface.ObjectManager.PlayerGuid, true)))
                {
                    return;
                }

                if (((WowInterface.ObjectManager.Player.HasBuffByName(sealOfVengeanceSpell) || WowInterface.ObjectManager.Player.HasBuffByName(sealOfWisdomSpell))
                        && TryCastSpell(judgementOfLightSpell, WowInterface.ObjectManager.TargetGuid, true))
                    || TryCastSpell(avengingWrathSpell, 0, true)
                    || (WowInterface.ObjectManager.Player.ManaPercentage < 80.0
                        && TryCastSpell(divinePleaSpell, 0, true)))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target != null)
                {
                    if ((WowInterface.ObjectManager.Player.HealthPercentage < 20.0
                            && TryCastSpell(hammerOfWrathSpell, WowInterface.ObjectManager.TargetGuid, true))
                        || TryCastSpell(crusaderStrikeSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || TryCastSpell(divineStormSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || TryCastSpell(consecrationSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || TryCastSpell(exorcismSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || TryCastSpell(holyWrathSpell, WowInterface.ObjectManager.TargetGuid, true))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }
    }
}