using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
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
    public class WarriorProtection : BasicCombatClass
    {
        public WarriorProtection(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { commandingShoutSpell, () => CastSpellIfPossible(commandingShoutSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { demoralizingShoutSpell, () => CastSpellIfPossible(demoralizingShoutSpell, WowInterface.ObjectManager.TargetGuid, true) },
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => (CastSpellIfPossibleWarrior(shieldBashSpell, defensiveStanceSpell, x.Guid, true)) },
                { 1, (x) => CastSpellIfPossible(concussionBlowSpell, x.Guid, true) }
            };

            HeroicStrikeEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Warrior;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "Leveling ready CombatClass for the Protection Warrior spec. For Dungeons and Questing";

        public override string Displayname => "Warrior Protection";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStaminaComparator(new List<ArmorType>()
        {
             ArmorType.IDOLS,
             ArmorType.LIBRAMS,
             ArmorType.SIGILS,
             ArmorType.TOTEMS,
             ArmorType.CLOTH,
             ArmorType.LEATHER
        }, new List<WeaponType>()
        {
            WeaponType.TWOHANDED_SWORDS,
            WeaponType.TWOHANDED_MACES,
            WeaponType.TWOHANDED_AXES,
            WeaponType.MISCELLANEOUS,
            WeaponType.STAVES,
            WeaponType.POLEARMS,
            WeaponType.THROWN,
            WeaponType.WANDS,
            WeaponType.DAGGERS
        });

        public override CombatClassRole Role => CombatClassRole.Tank;

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

        public override bool WalkBehindEnemy => false;

        private TimegatedEvent HeroicStrikeEvent { get; }

        public override void ExecuteCC()
        {
            if (WowInterface.ObjectManager.Target != null)
            {
                if ((WowInterface.ObjectManager.Player.IsFleeing
                    || WowInterface.ObjectManager.Player.IsDazed
                    || WowInterface.ObjectManager.Player.IsDisarmed)
                    && CastSpellIfPossible(berserkerRageSpell, 0, false))
                {
                    return;
                }

                double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                if (distanceToTarget > 8.0)
                {
                    if (CastSpellIfPossibleWarrior(chargeSpell, battleStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        return;
                    }
                }
                else
                {
                    int nearEnemies = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(WowInterface.ObjectManager.Player.Position, 10.0).Count;

                    if ((nearEnemies > 2 || WowInterface.ObjectManager.Player.Rage > 40)
                        && CastSpellIfPossibleWarrior(thunderClapSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid
                        && (WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 10).Count() > 3
                            && CastSpellIfPossible(challengingShoutSpell, 0, true))
                        || CastSpellIfPossibleWarrior(tauntSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 25
                        && CastSpellIfPossibleWarrior(retaliationSpell, battleStanceSpell, 0))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 40
                        && (CastSpellIfPossible(lastStandSpell, 0)
                            || CastSpellIfPossibleWarrior(shieldWallSpell, defensiveStanceSpell, 0)
                            || CastSpellIfPossibleWarrior(shieldBlockSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid, true)))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Target.IsCasting
                        && (CastSpellIfPossibleWarrior(shieldBashSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid)
                            || CastSpellIfPossible(spellReflectionSpell, 0)))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage > 50
                        && CastSpellIfPossible(bloodrageSpell, 0))
                    {
                        return;
                    }

                    if (CastSpellIfPossible(berserkerRageSpell, 0, true)
                        || CastSpellIfPossible(shieldSlamSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || CastSpellIfPossible(mockingBlowSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || ((nearEnemies > 2 || WowInterface.ObjectManager.Player.Rage > 40)
                            && CastSpellIfPossible(shockwaveSpell, WowInterface.ObjectManager.Target.Guid, true))
                        || CastSpellIfPossible(devastateSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || CastSpellIfPossibleWarrior(revengeSpell, defensiveStanceSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || (WowInterface.ObjectManager.Player.Rage > 40
                            && HeroicStrikeEvent.Run()
                            && CastSpellIfPossible(heroicStrikeSpell, WowInterface.ObjectManager.Target.Guid, true)))
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick())
            {
                return;
            }
        }
    }
}