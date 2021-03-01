using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class WarriorArms : BasicCombatClass
    {
        public WarriorArms(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(battleShoutSpell, () => TryCastSpell(battleShoutSpell, 0, true)));

            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(hamstringSpell, () => WowInterface.ObjectManager.Target?.Type == WowObjectType.Player && TryCastSpell(hamstringSpell, WowInterface.ObjectManager.TargetGuid, true)));
            TargetAuraManager.Jobs.Add(new KeepActiveAuraJob(rendSpell, () => WowInterface.ObjectManager.Target?.Type == WowObjectType.Player && WowInterface.ObjectManager.Player.Rage > 75 && TryCastSpell(rendSpell, WowInterface.ObjectManager.TargetGuid, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellWarrior(intimidatingShoutSpell, berserkerStanceSpell, x.Guid, true) },
                { 1, (x) => TryCastSpellWarrior(intimidatingShoutSpell, battleStanceSpell, x.Guid, true) }
            };

            HeroicStrikeEvent = new(TimeSpan.FromSeconds(2));
        }

        public override string Description => "FCFS based CombatClass for the Arms Warrior spec.";

        public override string Displayname => "Warrior Arms";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicStrengthComparator(new() { WowArmorType.SHIELDS }, new() { WowWeaponType.ONEHANDED_SWORDS, WowWeaponType.ONEHANDED_MACES, WowWeaponType.ONEHANDED_AXES });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 1, new(1, 1, 3) },
                { 3, new(1, 3, 2) },
                { 4, new(1, 4, 2) },
                { 6, new(1, 6, 3) },
                { 7, new(1, 7, 2) },
                { 8, new(1, 8, 1) },
                { 9, new(1, 9, 2) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 3) },
                { 12, new(1, 12, 3) },
                { 13, new(1, 13, 5) },
                { 14, new(1, 14, 1) },
                { 17, new(1, 17, 2) },
                { 19, new(1, 19, 2) },
                { 21, new(1, 21, 1) },
                { 22, new(1, 22, 2) },
                { 24, new(1, 24, 1) },
                { 25, new(1, 25, 3) },
                { 26, new(1, 26, 2) },
                { 27, new(1, 27, 3) },
                { 28, new(1, 28, 1) },
                { 29, new(1, 29, 2) },
                { 30, new(1, 30, 5) },
                { 31, new(1, 31, 1) },
            },
            Tree2 = new()
            {
                { 1, new(2, 1, 3) },
                { 2, new(2, 2, 2) },
                { 3, new(2, 3, 5) },
                { 5, new(2, 5, 5) },
                { 7, new(2, 7, 1) },
            },
            Tree3 = new(),
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => false;

        public override WowClass WowClass => WowClass.Warrior;

        private TimegatedEvent HeroicStrikeEvent { get; }

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
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