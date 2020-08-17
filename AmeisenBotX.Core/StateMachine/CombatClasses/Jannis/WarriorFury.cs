using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class WarriorFury : BasicCombatClass
    {
        public WarriorFury(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { battleShoutSpell, () => CastSpellIfPossible(battleShoutSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { hamstringSpell, () => WowInterface.ObjectManager.Target.Type == WowObjectType.Player && CastSpellIfPossible(hamstringSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { rendSpell, () => WowInterface.ObjectManager.Target.Type == WowObjectType.Player && WowInterface.ObjectManager.Player.Rage > 75 && CastSpellIfPossible(rendSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossibleWarrior(intimidatingShoutSpell, berserkerStanceSpell, x.Guid, true) },
                { 1, (x) => CastSpellIfPossibleWarrior(intimidatingShoutSpell, battleStanceSpell, x.Guid, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass WowClass => WowClass.Warrior;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Fury Warrior spec.";

        public override string Displayname => "Warrior Fury";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 3) },
                { 3, new Talent(1, 3, 2) },
                { 5, new Talent(1, 5, 2) },
                { 6, new Talent(1, 6, 3) },
                { 9, new Talent(1, 9, 2) },
                { 10, new Talent(1, 10, 3) },
                { 11, new Talent(1, 11, 3) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 3) },
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 6, new Talent(2, 6, 3) },
                { 10, new Talent(2, 10, 5) },
                { 13, new Talent(2, 13, 3) },
                { 14, new Talent(2, 14, 1) },
                { 16, new Talent(2, 16, 1) },
                { 17, new Talent(2, 17, 5) },
                { 18, new Talent(2, 18, 3) },
                { 19, new Talent(2, 19, 1) },
                { 20, new Talent(2, 20, 2) },
                { 22, new Talent(2, 22, 5) },
                { 23, new Talent(2, 23, 1) },
                { 24, new Talent(2, 24, 1) },
                { 25, new Talent(2, 25, 3) },
                { 26, new Talent(2, 26, 5) },
                { 27, new Talent(2, 27, 1) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override void ExecuteCC()
        {
            if (SelectTarget(DpsTargetManager))
            {
                if (WowInterface.ObjectManager.Target != null)
                {
                    double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                    if (distanceToTarget > 5.0)
                    {
                        if (CastSpellIfPossibleWarrior(chargeSpell, battleStanceSpell, WowInterface.ObjectManager.Target.Guid, true)
                            || (CastSpellIfPossible(berserkerRageSpell, WowInterface.ObjectManager.Target.Guid, true) && CastSpellIfPossibleWarrior(interceptSpell, berserkerStanceSpell, WowInterface.ObjectManager.Target.Guid, true)))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (WowInterface.ObjectManager.Player.HealthPercentage < 50.0
                            && CastSpellIfPossible(enragedRegenerationSpell, 0, true))
                        {
                            return;
                        }

                        if (CastSpellIfPossible(berserkerRageSpell, WowInterface.ObjectManager.Target.Guid, true))
                        {
                            return;
                        }

                        if (CastSpellIfPossibleWarrior(bloodthirstSpell, berserkerStanceSpell, WowInterface.ObjectManager.Target.Guid, true)
                            && CastSpellIfPossibleWarrior(whirlwindSpell, berserkerStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
                        {
                            return;
                        }

                        if (WowInterface.ObjectManager.Player.HasBuffByName($"{slamSpell}!")
                           && CastSpellIfPossibleWarrior(slamSpell, berserkerStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
                        {
                            return;
                        }

                        if (CastSpellIfPossible(bloodrageSpell, WowInterface.ObjectManager.Target.Guid, true)
                            && CastSpellIfPossible(recklessnessSpell, WowInterface.ObjectManager.Target.Guid, true))
                        {
                            return;
                        }

                        if ((WowInterface.ObjectManager.Target.HealthPercentage < 20)
                           && CastSpellIfPossibleWarrior(executeSpell, berserkerStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
                        {
                            return;
                        }

                        if ((WowInterface.ObjectManager.Player.Rage > 25 && WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 5).Count() > 2 && CastSpellIfPossibleWarrior(cleaveSpell, berserkerStanceSpell, 0, true))
                            || (WowInterface.ObjectManager.Player.Rage > 35 && CastSpellIfPossibleWarrior(heroicStrikeSpell, berserkerStanceSpell, WowInterface.ObjectManager.TargetGuid, true)))
                        {
                            return;
                        }
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