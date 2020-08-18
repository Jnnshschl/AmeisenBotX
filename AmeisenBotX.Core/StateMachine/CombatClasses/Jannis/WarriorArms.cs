using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
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
        public WarriorArms(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { battleShoutSpell, () => TryCastSpell(battleShoutSpell, 0, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { hamstringSpell, () => WowInterface.ObjectManager.Target.Type == WowObjectType.Player && TryCastSpell(hamstringSpell, WowInterface.ObjectManager.TargetGuid, true) },
                { rendSpell, () => WowInterface.ObjectManager.Target.Type == WowObjectType.Player && WowInterface.ObjectManager.Player.Rage > 75 && TryCastSpell(rendSpell, WowInterface.ObjectManager.TargetGuid, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpellWarrior(intimidatingShoutSpell, berserkerStanceSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellWarrior(intimidatingShoutSpell, battleStanceSpell, x.Guid, true) }
            };

            HeroicStrikeEvent = new TimegatedEvent(TimeSpan.FromSeconds(2));
        }

        public override string Description => "FCFS based CombatClass for the Arms Warrior spec.";

        public override string Displayname => "Warrior Arms";

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

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Warrior;

        private TimegatedEvent HeroicStrikeEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(DpsTargetManager))
            {
                if (WowInterface.ObjectManager.Target != null)
                {
                    double distanceToTarget = WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

                    if (distanceToTarget > 3.0)
                    {
                        if (TryCastSpellWarrior(chargeSpell, battleStanceSpell, WowInterface.ObjectManager.Target.Guid, true)
                            || TryCastSpellWarrior(interceptSpell, berserkerStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if ((WowInterface.ObjectManager.Target.HealthPercentage < 20 || WowInterface.ObjectManager.Target.HasBuffByName("Sudden Death"))
                           && TryCastSpellWarrior(executeSpell, battleStanceSpell, WowInterface.ObjectManager.Target.Guid, true))
                        {
                            return;
                        }

                        if ((WowInterface.ObjectManager.WowObjects.OfType<WowUnit>().Where(e => WowInterface.ObjectManager.Target.Position.GetDistance(e.Position) < 8).Count() > 2 && TryCastSpell(bladestormSpell, 0, true))
                            || TryCastSpellWarrior(overpowerSpell, battleStanceSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || TryCastSpellWarrior(mortalStrikeSpell, battleStanceSpell, WowInterface.ObjectManager.TargetGuid, true)
                            || (HeroicStrikeEvent.Run() && TryCastSpellWarrior(heroicStrikeSpell, battleStanceSpell, WowInterface.ObjectManager.TargetGuid, true)))
                        {
                            return;
                        }
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