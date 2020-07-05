using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class WarriorProtection : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string battleStanceSpell = "Battle Stance";
        private const string berserkerRageSpell = "Berserker Rage";
        private const string bloodrageSpell = "Bloodrage";
        private const string challengingShoutSpell = "Challenging Shout";
        private const string chargeSpell = "Charge";
        private const string commandingShoutSpell = "Commanding Shout";
        private const string concussionBlowSpell = "Concussion Blow";
        private const string defensiveStanceSpell = "Defensive Stance";
        private const string demoralizingShoutSpell = "Demoralizing Shout";
        private const string devastateSpell = "Devastate";
        private const string disarmSpell = "Disarm";
        private const string executeSpell = "Execute";
        private const string heroicStrikeSpell = "Heroic Strike";
        private const string heroicThrowSpell = "Heroic Throw";
        private const string lastStandSpell = "Last Stand";
        private const string mockingBlowSpell = "Mocking Blow";
        private const string revengeSpell = "Revenge";
        private const string shieldBashSpell = "Shield Bash";
        private const string shieldBlockSpell = "Shield Block";
        private const string shieldSlamSpell = "Shield Slam";
        private const string shieldWallSpell = "Shield Wall";
        private const string shockwaveSpell = "Shockwave";
        private const string spellReflectionSpell = "Spell Reflection";
        private const string tauntSpell = "Taunt";
        private const string thunderClapSpell = "Thunder Clap";
        private const string victoryRushSpell = "Victory Rush";
#pragma warning restore IDE0051

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
                { 0, (x) => (SwitchStance(defensiveStanceSpell) && CastSpellIfPossible(shieldBashSpell, x.Guid, true)) },
                { 1, (x) => CastSpellIfPossible(concussionBlowSpell, x.Guid, true) }
            };
        }

        public override string Author => "Jannis";

        public override bool WalkBehindEnemy => false;

        public override WowClass Class => WowClass.Warrior;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Protection Warrior spec.";

        public override string Displayname => "Warrior Protection";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override bool UseAutoAttacks => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStaminaComparator(null, new List<WeaponType>() { WeaponType.TWOHANDED_SWORDS, WeaponType.TWOHANDED_MACES, WeaponType.TWOHANDED_AXES, WeaponType.MISCELLANEOUS });

        public override CombatClassRole Role => CombatClassRole.Tank;

        public override string Version => "1.0";

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

        public override void ExecuteCC()
        {
            if (WowInterface.ObjectManager.Player.HealthPercentage < 30)
            {
                IWowItem healthstone = WowInterface.CharacterManager.Inventory.Items.FirstOrDefault(e => e.Id == 5512);
                if (healthstone != null)
                {
                    WowInterface.HookManager.UseItemByName(healthstone.Name);
                }
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
                        && (CastSpellIfPossible(shieldBashSpell, WowInterface.ObjectManager.Target.Guid)
                            || CastSpellIfPossible(spellReflectionSpell, 0)))
                    {
                        return;
                    }


                    if (WowInterface.ObjectManager.Player.HasBuffByName(victoryRushSpell))
                    {
                        if (CastSpellIfPossible(victoryRushSpell, WowInterface.ObjectManager.Target.Guid))
                        {
                            return;
                        }
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage > 50)
                    {
                        if (CastSpellIfPossible(bloodrageSpell, 0))
                        {
                            return;
                        }
                    }

                    if (WowInterface.ObjectManager.Player.HealthPercentage < 40)
                    {
                        if (CastSpellIfPossible(lastStandSpell, 0)
                            || CastSpellIfPossible(shieldWallSpell, 0))
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