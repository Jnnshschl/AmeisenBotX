using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class WarriorProtection : BasicCombatClass
    {
        public WarriorProtection(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(commandingShoutSpell, () => TryCastSpell(commandingShoutSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(demoralizingShoutSpell, () => TryCastSpell(demoralizingShoutSpell, WowInterface.Target.Guid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => (TryCastSpellWarrior(shieldBashSpell, defensiveStanceSpell, x.Guid, true)) },
                { 1, (x) => TryCastSpell(concussionBlowSpell, x.Guid, true) }
            };

            HeroicStrikeEvent = new(TimeSpan.FromSeconds(2));
        }

        public override string Description => "Leveling ready CombatClass for the Protection Warrior spec. For Dungeons and Questing";

        public override string Displayname => "Warrior Protection";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStaminaComparator(new()
        {
            WowArmorType.IDOLS,
            WowArmorType.LIBRAMS,
            WowArmorType.SIGILS,
            WowArmorType.TOTEMS,
            WowArmorType.CLOTH,
            WowArmorType.LEATHER
        }, new()
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

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 2, new(1, 2, 5) },
                { 4, new(1, 4, 2) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
            },
            Tree3 = new()
            {
                { 2, new(3, 2, 5) },
                { 3, new(3, 3, 3) },
                { 4, new(3, 4, 3) },
                { 5, new(3, 5, 5) },
                { 6, new(3, 6, 1) },
                { 7, new(3, 7, 2) },
                { 8, new(3, 8, 2) },
                { 9, new(3, 9, 5) },
                { 13, new(3, 13, 2) },
                { 14, new(3, 14, 1) },
                { 15, new(3, 15, 2) },
                { 16, new(3, 16, 5) },
                { 17, new(3, 17, 2) },
                { 18, new(3, 18, 1) },
                { 20, new(3, 20, 3) },
                { 22, new(3, 22, 1) },
                { 23, new(3, 23, 1) },
                { 24, new(3, 24, 3) },
                { 25, new(3, 25, 3) },
                { 26, new(3, 26, 2) },
                { 27, new(3, 27, 1) },
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

            if (SelectTarget(TargetProviderTank))
            {
                if ((WowInterface.Player.IsFleeing
                    || WowInterface.Player.IsDazed
                    || WowInterface.Player.IsDisarmed)
                    && TryCastSpell(berserkerRageSpell, 0, false))
                {
                    return;
                }

                double distanceToTarget = WowInterface.Target.Position.GetDistance(WowInterface.Player.Position);

                if (distanceToTarget > 8.0)
                {
                    if (TryCastSpellWarrior(chargeSpell, battleStanceSpell, WowInterface.Target.Guid, true))
                    {
                        return;
                    }
                }
                else
                {
                    if (WowInterface.Player.Rage > 40
                        && HeroicStrikeEvent.Run()
                        && TryCastSpell(heroicStrikeSpell, WowInterface.Target.Guid, true))
                    {
                        // do not return, hehe xd
                    }

                    int nearEnemies = WowInterface.Objects.GetNearEnemies<WowUnit>(WowInterface.NewWowInterface, WowInterface.Player.Position, 10.0f).Count();

                    if ((nearEnemies > 2 || WowInterface.Player.Rage > 40)
                        && TryCastSpellWarrior(thunderClapSpell, defensiveStanceSpell, WowInterface.Target.Guid, true))
                    {
                        return;
                    }

                    if (WowInterface.Target.TargetGuid != WowInterface.Player.Guid
                        && (WowInterface.Objects.WowObjects.OfType<WowUnit>().Where(e => WowInterface.Target.Position.GetDistance(e.Position) < 10.0).Count() > 3
                            && TryCastSpell(challengingShoutSpell, 0, true))
                        || TryCastSpellWarrior(tauntSpell, defensiveStanceSpell, WowInterface.Target.Guid))
                    {
                        return;
                    }

                    if (WowInterface.Player.HealthPercentage < 25.0
                        && TryCastSpellWarrior(retaliationSpell, battleStanceSpell, 0))
                    {
                        return;
                    }

                    if (WowInterface.Player.HealthPercentage < 40.0
                        && (TryCastSpell(lastStandSpell, 0)
                            || TryCastSpellWarrior(shieldWallSpell, defensiveStanceSpell, 0)
                            || TryCastSpellWarrior(shieldBlockSpell, defensiveStanceSpell, WowInterface.Target.Guid, true)))
                    {
                        return;
                    }

                    if (WowInterface.Target.IsCasting
                        && (TryCastSpellWarrior(shieldBashSpell, defensiveStanceSpell, WowInterface.Target.Guid)
                            || TryCastSpellWarrior(spellReflectionSpell, defensiveStanceSpell, 0)))
                    {
                        return;
                    }

                    if (WowInterface.Player.HealthPercentage > 50.0
                        && TryCastSpell(bloodrageSpell, 0))
                    {
                        return;
                    }

                    if (TryCastSpell(berserkerRageSpell, 0, true)
                        || TryCastSpell(shieldSlamSpell, WowInterface.Target.Guid, true)
                        || TryCastSpell(mockingBlowSpell, WowInterface.Target.Guid, true)
                        || ((nearEnemies > 2 || WowInterface.Player.Rage > 40)
                            && TryCastSpell(shockwaveSpell, WowInterface.Target.Guid, true))
                        || TryCastSpell(devastateSpell, WowInterface.Target.Guid, true)
                        || TryCastSpellWarrior(revengeSpell, defensiveStanceSpell, WowInterface.Target.Guid, true))
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