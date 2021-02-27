using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Utils.InterruptManager;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class WarriorProtection : BasicCombatClass
    {
        public WarriorProtection(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(commandingShoutSpell, () => TryCastSpell(commandingShoutSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(demoralizingShoutSpell, () => TryCastSpell(demoralizingShoutSpell, WowInterface.ObjectManager.TargetGuid, true)));

            InterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => (TryCastSpellWarrior(shieldBashSpell, defensiveStanceSpell, x.Guid, true)) },
                { 1, (x) => TryCastSpell(concussionBlowSpell, x.Guid, true) }
            };

            HeroicStrikeEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        public override string Description => "Leveling ready CombatClass for the Protection Warrior spec. For Dungeons and Questing";

        public override string Displayname => "Warrior Protection";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStaminaComparator(new List<WowArmorType>()
        {
             WowArmorType.IDOLS,
             WowArmorType.LIBRAMS,
             WowArmorType.SIGILS,
             WowArmorType.TOTEMS,
             WowArmorType.CLOTH,
             WowArmorType.LEATHER
        }, new List<WowWeaponType>()
        {
            WowWeaponType.TWOHANDED_SWORDS,
            WowWeaponType.TWOHANDED_MACES,
            WowWeaponType.TWOHANDED_AXES,
            WowWeaponType.MISCELLANEOUS,
            WowWeaponType.STAVES,
            WowWeaponType.POLEARMS,
            WowWeaponType.THROWN,
            WowWeaponType.WANDS,
            WowWeaponType.DAGGERS
        });

        public override WowRole Role => WowRole.Tank;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 3) },
                { 2, new Talent(1, 2, 5) },
                { 4, new Talent(1, 4, 2) },
                { 9, new Talent(1, 9, 2) },
                { 10, new Talent(1, 10, 3) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 3) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 2, new Talent(3, 2, 5) },
                { 3, new Talent(3, 3, 3) },
                { 4, new Talent(3, 4, 3) },
                { 5, new Talent(3, 5, 5) },
                { 6, new Talent(3, 6, 1) },
                { 7, new Talent(3, 7, 2) },
                { 8, new Talent(3, 8, 2) },
                { 9, new Talent(3, 9, 5) },
                { 13, new Talent(3, 13, 2) },
                { 14, new Talent(3, 14, 1) },
                { 15, new Talent(3, 15, 2) },
                { 16, new Talent(3, 16, 5) },
                { 17, new Talent(3, 17, 2) },
                { 18, new Talent(3, 18, 1) },
                { 20, new Talent(3, 20, 3) },
                { 22, new Talent(3, 22, 1) },
                { 23, new Talent(3, 23, 1) },
                { 24, new Talent(3, 24, 3) },
                { 25, new Talent(3, 25, 3) },
                { 26, new Talent(3, 26, 2) },
                { 27, new Talent(3, 27, 1) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.1";

        public override bool WalkBehindEnemy => true;

        public override WowClass WowClass => WowClass.Warrior;

        private TimegatedEvent HeroicStrikeEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerTank))
            {
                if ((WowInterface.ObjectManager.Player.IsFleeing
                    || WowInterface.ObjectManager.Player.IsDazed
                    || WowInterface.ObjectManager.Player.IsDisarmed)
                    && TryCastSpell(berserkerRageSpell, 0, false))
                {
                    return;
                }

                double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                if (distanceToTarget > 8.0)
                {
                    if (TryCastSpellWarrior(chargeSpell, battleStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        return;
                    }
                }
                else
                {
                    if (WowInterface.ObjectManager.Player.Rage > 40
                        && HeroicStrikeEvent.Run()
                        && TryCastSpell(heroicStrikeSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        // do not return, hehe xd
                    }

                    int nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 10.0).Count();

                    if ((nearEnemies > 2 || WowInterface.ObjectManager.Player.Rage > 40)
                        && TryCastSpellWarrior(thunderClapSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                        && (WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 10.0).Count() > 3
                            && TryCastSpell(challengingShoutSpell, 0, true))
                        || TryCastSpellWarrior(tauntSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 25.0
                        && TryCastSpellWarrior(retaliationSpell, battleStanceSpell, 0))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 40.0
                        && (TryCastSpell(lastStandSpell, 0)
                            || TryCastSpellWarrior(shieldWallSpell, defensiveStanceSpell, 0)
                            || TryCastSpellWarrior(shieldBlockSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid, true)))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Target.IsCasting
                        && (TryCastSpellWarrior(shieldBashSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid)
                            || TryCastSpellWarrior(spellReflectionSpell, defensiveStanceSpell, 0)))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage > 50.0
                        && TryCastSpell(bloodrageSpell, 0))
                    {
                        return;
                    }

                    if (TryCastSpell(berserkerRageSpell, 0, true)
                        || TryCastSpell(shieldSlamSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || TryCastSpell(mockingBlowSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || ((nearEnemies > 2 || WowInterface.ObjectManager.Player.Rage > 40)
                            && TryCastSpell(shockwaveSpell, WowInterface.ObjectManager.Target.Guid, true))
                        || TryCastSpell(devastateSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || TryCastSpellWarrior(revengeSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
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