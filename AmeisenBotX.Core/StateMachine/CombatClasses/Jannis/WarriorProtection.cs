using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
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
        // author: Jannis Höschele

        private readonly string battleStanceSpell = "Battle Stance";
        private readonly string berserkerRageSpell = "Berserker Rage";
        private readonly string challengingShoutSpell = "Challenging Shout";
        private readonly string chargeSpell = "Charge";
        private readonly string commandingShoutSpell = "Commanding Shout";
        private readonly string concussionBlowSpell = "Concussion Blow";
        private readonly string defensiveStanceSpell = "Defensive Stance";
        private readonly string demoralizingShoutSpell = "Demoralizing Shout";
        private readonly string devastateSpell = "Devastate";
        private readonly string disarmSpell = "Disarm";
        private readonly string executeSpell = "Execute";
        private readonly string heroicStrikeSpell = "Heroic Strike";
        private readonly string heroicThrowSpell = "Heroic Throw";
        private readonly string lastStandSpell = "Last Stand";
        private readonly string mockingBlowSpell = "Mocking Blow";
        private readonly string revengeSpell = "Revenge";
        private readonly string shieldBashSpell = "Shield Bash";
        private readonly string shieldBlockSpell = "Shield Block";
        private readonly string shieldSlamSpell = "Shield Slam";
        private readonly string shieldWallSpell = "Shield Wall";
        private readonly string shockwaveSpell = "Shockwave";
        private readonly string spellReflectionSpell = "Spell Reflection";
        private readonly string tauntSpell = "Taunt";
        private readonly string thunderClapSpell = "Thunder Clap";

        public WarriorProtection(WowInterface wowInterface) : base(wowInterface)
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
                { 0, (x) => (SwitchStance(defensiveStanceSpell) && CastSpellIfPossible(shieldBashSpell, x.Guid, true)) },
                { 1, (x) => CastSpellIfPossible(concussionBlowSpell, x.Guid, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Warrior;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Protection Warrior spec.";

        public override string Displayname => "Warrior Protection";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => true;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicArmorComparator(null, new List<WeaponType>() { WeaponType.TWOHANDED_SWORDS, WeaponType.TWOHANDED_MACES, WeaponType.TWOHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Tank;

        public override string Version => "1.0";

        private DateTime LastAutoAttackCheck { get; set; }

        public override void ExecuteCC()
        {
            if (DateTime.Now - LastAutoAttackCheck > TimeSpan.FromSeconds(4) && WowInterface.ObjectManager.TargetGuid > 0 && !WowInterface.ObjectManager.Player.IsAutoAttacking)
            {
                LastAutoAttackCheck = DateTime.Now;
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick())
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                if (distanceToTarget > 3)
                {
                    if (CastSpellIfPossible(heroicThrowSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || (SwitchStance(battleStanceSpell) && CastSpellIfPossible(chargeSpell, WowInterface.ObjectManager.Target.Guid, true)))
                    {
                        return;
                    }
                }
                else
                {
                    if (WowInterface.ObjectManager.Target.TargetGuid != WowInterface.ObjectManager.PlayerGuid)
                    {
                        if ((SwitchStance(defensiveStanceSpell) && CastSpellIfPossible(tauntSpell, WowInterface.ObjectManager.Target.Guid))
                            || (WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 10).Count() > 3 && CastSpellIfPossible(challengingShoutSpell, 0, true)))
                        {
                            return;
                        }
                    }

                    if (WowInterface.ObjectManager.Target.IsCasting
                        && CastSpellIfPossible(spellReflectionSpell, 0))
                    {
                        return;
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 40)
                    {
                        if (CastSpellIfPossible(lastStandSpell, 0)
                            && CastSpellIfPossible(shieldWallSpell, 0))
                        {
                            return;
                        }
                    }

                    if (CastSpellIfPossible(berserkerRageSpell, 0, true)
                        || (SwitchStance(defensiveStanceSpell) && CastSpellIfPossible(revengeSpell, WowInterface.ObjectManager.Target.Guid, true))
                        || (SwitchStance(defensiveStanceSpell) && CastSpellIfPossible(shieldBlockSpell, WowInterface.ObjectManager.Target.Guid, true))
                        || CastSpellIfPossible(shieldSlamSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || CastSpellIfPossible(thunderClapSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || CastSpellIfPossible(mockingBlowSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || (WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 5).Count() > 2 && CastSpellIfPossible(shockwaveSpell, WowInterface.ObjectManager.Target.Guid, true))
                        || (WowInterface.ObjectManager.Target.HealthPercentage < 20) && CastSpellIfPossible(executeSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || CastSpellIfPossible(devastateSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || CastSpellIfPossible(heroicStrikeSpell, WowInterface.ObjectManager.TargetGuid, true))
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

        private bool SwitchStance(string stanceName)
        {
            if (WowInterface.ObjectManager.Player.HasBuffByName(stanceName))
            {
                return true;
            }
            else
            {
                if (CastSpellIfPossible(stanceName, WowInterface.ObjectManager.PlayerGuid))
                {
                    return true;
                }
            }

            return false;
        }
    }
}