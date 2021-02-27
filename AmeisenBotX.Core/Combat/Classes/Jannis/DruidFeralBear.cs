using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Utils.InterruptManager;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class DruidFeralBear : BasicCombatClass
    {
        public DruidFeralBear(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(markOfTheWildSpell, () => TryCastSpell(markOfTheWildSpell, WowInterface.ObjectManager.PlayerGuid, true, 0, true)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(direBearFormSpell, () => TryCastSpell(direBearFormSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(mangleBearSpell, () => TryCastSpell(mangleBearSpell, WowInterface.ObjectManager.TargetGuid, true)));

            InterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpell(bashSpell, x.Guid, true) },
            };

            GroupAuraManager.SpellsToKeepActiveOnParty.Add((markOfTheWildSpell, (spellName, guid) => TryCastSpell(spellName, guid, true)));
        }

        public override string Description => "FCFS based CombatClass for the Feral (Bear) Druid spec.";

        public override string Displayname => "Druid Feral Bear";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicArmorComparator(new List<WowArmorType>() { WowArmorType.SHIELDS }, new List<WowWeaponType>() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Tank;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>(),
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

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Druid;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
                double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                if (distanceToTarget > 9.0
                    && TryCastSpell(feralChargeBearSpell, WowInterface.ObjectManager.Target.Guid, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Player.HealthPercentage < 40
                    && TryCastSpell(survivalInstinctsSpell, 0, true))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                    && TryCastSpell(growlSpell, 0, true))
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

                int nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 10).Count();

                if ((WowInterface.ObjectManager.Player.HealthPercentage > 80
                        && TryCastSpell(enrageSpell, 0, true))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 70
                        && TryCastSpell(barkskinSpell, 0, true))
                    || (WowInterface.ObjectManager.Player.HealthPercentage < 75
                        && TryCastSpell(frenziedRegenerationSpell, 0, true))
                    || (nearEnemies > 2 && TryCastSpell(challengingRoarSpell, 0, true))
                    || TryCastSpell(lacerateSpell, WowInterface.ObjectManager.TargetGuid, true)
                    || (nearEnemies > 2 && TryCastSpell(swipeSpell, 0, true))
                    || TryCastSpell(mangleBearSpell, WowInterface.ObjectManager.TargetGuid, true))
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