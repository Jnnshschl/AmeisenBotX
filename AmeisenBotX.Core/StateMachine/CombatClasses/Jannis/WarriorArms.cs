using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
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
    public class WarriorArms : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string battleStanceSpell = "Battle Stance";
        private readonly string bladestormSpell = "Bladestorm";
        private readonly string chargeSpell = "Charge";
        private readonly string commandingShoutSpell = "Commanding Shout";
        private readonly string disarmSpell = "Disarm";
        private readonly string executeSpell = "Execute";
        private readonly string hamstringSpell = "Hamstring";
        private readonly string heroicStrikeSpell = "Heroic Strike";
        private readonly string interceptSpell = "Intercept";
        private readonly string intimidatingShoutSpell = "Intimidating Shout";
        private readonly string mortalStrikeSpell = "Mortal Strike";
        private readonly string overpowerSpell = "Overpower";
        private readonly string rendSpell = "Rend";

        public WarriorArms(WowInterface wowInterface) : base(wowInterface)
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

        public override WowClass Class => WowClass.Warrior;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Arms Warrior spec.";

        public override string Displayname => "Warrior Arms";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new List<ArmorType>() { ArmorType.SHIEDLS }, new List<WeaponType>() { WeaponType.ONEHANDED_SWORDS, WeaponType.ONEHANDED_MACES, WeaponType.ONEHANDED_AXES });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private DateTime LastAutoAttackCheck { get; set; }

        public override void ExecuteCC()
        {
            if (DateTime.Now - LastAutoAttackCheck > TimeSpan.FromSeconds(4) && !WowInterface.ObjectManager.Player.IsAutoAttacking)
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