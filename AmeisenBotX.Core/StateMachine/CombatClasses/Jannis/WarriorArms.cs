using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
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
    public class WarriorArms : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string battleStanceSpell = "Battle Stance";
        private const string bladestormSpell = "Bladestorm";
        private const string chargeSpell = "Charge";
        private const string commandingShoutSpell = "Commanding Shout";
        private const string disarmSpell = "Disarm";
        private const string executeSpell = "Execute";
        private const string hamstringSpell = "Hamstring";
        private const string heroicStrikeSpell = "Heroic Strike";
        private const string interceptSpell = "Intercept";
        private const string intimidatingShoutSpell = "Intimidating Shout";
        private const string mortalStrikeSpell = "Mortal Strike";
        private const string overpowerSpell = "Overpower";
        private const string rendSpell = "Rend";
#pragma warning restore IDE0051

        public WarriorArms(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { battleStanceSpell, () => CastSpellIfPossible(battleStanceSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { hamstringSpell, () => CastSpellIfPossible(hamstringSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { rendSpell, () => CastSpellIfPossible(rendSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossible(intimidatingShoutSpell, x.Guid, true) }
            };
        }

        public override string Author => "Jannis";

        public override bool WalkBehindEnemy => false;

        public override WowClass Class => WowClass.Warrior;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Arms Warrior spec.";

        public override string Displayname => "Warrior Arms";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override bool UseAutoAttacks => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIELDS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(1, 1, 3) },
                { 3, new Talent(1, 3, 2) },
                { 4, new Talent(1, 4, 2) },
                { 6, new Talent(1, 6, 3) },
                { 7, new Talent(1, 7, 2) },
                { 8, new Talent(1, 8, 1) },
                { 9, new Talent(1, 9, 2) },
                { 10, new Talent(1, 10, 3) },
                { 11, new Talent(1, 11, 3) },
                { 12, new Talent(1, 12, 3) },
                { 13, new Talent(1, 13, 5) },
                { 14, new Talent(1, 14, 1) },
                { 17, new Talent(1, 17, 2) },
                { 19, new Talent(1, 19, 2) },
                { 21, new Talent(1, 21, 1) },
                { 22, new Talent(1, 22, 2) },
                { 24, new Talent(1, 24, 1) },
                { 25, new Talent(1, 25, 3) },
                { 26, new Talent(1, 26, 2) },
                { 27, new Talent(1, 27, 3) },
                { 28, new Talent(1, 28, 1) },
                { 29, new Talent(1, 29, 2) },
                { 30, new Talent(1, 30, 5) },
                { 31, new Talent(1, 31, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(2, 1, 3) },
                { 2, new Talent(2, 2, 2) },
                { 3, new Talent(2, 3, 5) },
                { 5, new Talent(2, 5, 5) },
                { 7, new Talent(2, 7, 1) },
            },
            Tree3 = new Dictionary<int, Talent>(),
        };

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run() && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                if (distanceToTarget > 3)
                {
                    if (CastSpellIfPossible(chargeSpell, WowInterface.ObjectManager.Target.Guid, true)
                        || CastSpellIfPossible(interceptSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        return;
                    }
                }
                else
                {
                    if ((WowInterface.ObjectManager.Target.HealthPercentage < 20 || WowInterface.ObjectManager.Target.HasBuffByName("Sudden Death"))
                       && CastSpellIfPossible(executeSpell, WowInterface.ObjectManager.Target.Guid, true))
                    {
                        return;
                    }

                    if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 8).Count() > 2 && CastSpellIfPossible(bladestormSpell, 0, true))
                        || CastSpellIfPossible(overpowerSpell, WowInterface.ObjectManager.TargetGuid, true)
                        || CastSpellIfPossible(mortalStrikeSpell, WowInterface.ObjectManager.TargetGuid, true)
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
    }
}