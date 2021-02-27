using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System.Collections.Generic;
using static AmeisenBotX.Core.Utils.InterruptManager;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class DruidFeralCat : BasicCombatClass
    {
        public DruidFeralCat(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(markOfTheWildSpell, () => TryCastSpell(markOfTheWildSpell, WowInterface.ObjectManager.PlayerGuid, true, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(catFormSpell, () => TryCastSpell(catFormSpell, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(savageRoarSpell, () => TryCastSpellRogue(savageRoarSpell, WowInterface.ObjectManager.TargetGuid, true, true, 1)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(ripSpell, () => WowInterface.ObjectManager.Player.ComboPoints == 5 && TryCastSpellRogue(ripSpell, WowInterface.ObjectManager.TargetGuid, true, true, 5)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(rakeSpell, () => TryCastSpell(rakeSpell, WowInterface.ObjectManager.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(mangleCatSpell, () => TryCastSpell(mangleCatSpell, WowInterface.ObjectManager.TargetGuid, true)));

            InterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpell(faerieFireSpell, x.Guid, true) },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Feral (Cat) Druid spec.";

        public override string Displayname => "Druid Feral Cat";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicAgilityComparator(new List<WowArmorType>() { WowArmorType.SHIELDS }, new List<WowWeaponType>() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 5) },
                { 2, new Talent(2, 2, 5) },
                { 4, new Talent(2, 4, 2) },
                { 6, new Talent(2, 6, 2) },
                { 7, new Talent(2, 7, 1) },
                { 8, new Talent(2, 8, 3) },
                { 9, new Talent(2, 9, 2) },
                { 10, new Talent(2, 10, 3) },
                { 11, new Talent(2, 11, 2) },
                { 12, new Talent(2, 12, 2) },
                { 14, new Talent(2, 14, 1) },
                { 17, new Talent(2, 17, 5) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 1) },
                { 20, new Talent(2, 20, 2) },
                { 23, new Talent(2, 23, 3) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 1) },
                { 28, new Talent(2, 28, 5) },
                { 29, new Talent(2, 29, 1) },
                { 30, new Talent(2, 30, 1) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 2) },
                { 3, new Talent(3, 3, 5) },
                { 4, new Talent(3, 4, 5) },
                { 6, new Talent(3, 6, 3) },
                { 8, new Talent(3, 8, 1) },
                { 9, new Talent(3, 9, 2) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => true;

        public override WowClass WowClass => WowClass.Druid;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
                double distanceToTarget = WowInterface.ObjectManager.Player.Position.GetDistance(WowInterface.ObjectManager.Target.Position);

                if (distanceToTarget > 9.0
                    && TryCastSpell(feralChargeBearSpell, WowInterface.ObjectManager.Target.Guid, true))
                {
                    return;
                }

                if (distanceToTarget > 8.0
                    && TryCastSpell(dashSpell, 0))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Player.HealthPercentage < 40
                    && TryCastSpell(survivalInstinctsSpell, 0, true))
                {
                    return;
                }

                if (TryCastSpell(berserkSpell, 0))
                {
                    return;
                }

                if (NeedToHealMySelf())
                {
                    return;
                }

                if ((WowInterface.ObjectManager.Player.EnergyPercentage > 70
                        && TryCastSpell(berserkSpell, 0))
                    || (WowInterface.ObjectManager.Player.Energy < 30
                        && TryCastSpell(tigersFurySpell, 0))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 70
                        && TryCastSpell(barkskinSpell, 0, true))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 35
                        && TryCastSpell(survivalInstinctsSpell, 0, true))
                    || (WowInterface.ObjectManager.Player.ComboPoints == 5
                        && TryCastSpellRogue(ferociousBiteSpell, WowInterface.ObjectManager.Target.Guid, true, true, 5))
                    || TryCastSpell(shredSpell, WowInterface.ObjectManager.Target.Guid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();

            if (NeedToHealMySelf())
            {
                return;
            }
        }

        private bool NeedToHealMySelf()
        {
            if (WowInterface.ObjectManager.Player.HealthPercentage < 60
                && !WowInterface.ObjectManager.Player.HasBuffByName(rejuvenationSpell)
                && TryCastSpell(rejuvenationSpell, 0, true))
            {
                return true;
            }

            if (WowInterface.ObjectManager.Player.HealthPercentage < 40
                && TryCastSpell(healingTouchSpell, 0, true))
            {
                return true;
            }

            return false;
        }
    }
}