using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class RogueAssassination : BasicCombatClass
    {
        public RogueAssassination(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { sliceAndDiceSpell, () => CastSpellIfPossibleRogue(sliceAndDiceSpell, 0, true, true, 1) },
                { coldBloodSpell, () => CastSpellIfPossibleRogue(coldBloodSpell, 0, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossibleRogue(kickSpell, x.Guid, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Rogue;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Assasination Rogue spec.";

        public override string Displayname => "[WIP] Rogue Assasination";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicAgilityComparator(new List<ArmorType>() { ArmorType.SHIELDS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override TalentTree Talents { get; } = new TalentTree()
        {
            Tree1 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(1, 3, 2) },
                { 4, new Talent(1, 4, 5) },
                { 5, new Talent(1, 5, 2) },
                { 6, new Talent(1, 6, 3) },
                { 9, new Talent(1, 9, 5) },
                { 10, new Talent(1, 10, 3) },
                { 11, new Talent(1, 11, 5) },
                { 13, new Talent(1, 13, 1) },
                { 16, new Talent(1, 16, 5) },
                { 17, new Talent(1, 17, 2) },
                { 19, new Talent(1, 19, 1) },
                { 21, new Talent(1, 21, 3) },
                { 22, new Talent(1, 22, 3) },
                { 23, new Talent(1, 23, 3) },
                { 24, new Talent(1, 24, 1) },
                { 26, new Talent(1, 26, 5) },
                { 27, new Talent(1, 27, 1) },
            },
            Tree2 = new Dictionary<int, Talent>()
            {
                { 3, new Talent(2, 3, 5) },
                { 6, new Talent(2, 6, 5) },
                { 9, new Talent(2, 9, 3) },
            },
            Tree3 = new Dictionary<int, Talent>()
            {
                { 1, new Talent(3, 1, 5) },
                { 3, new Talent(3, 3, 2) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => true;

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run() && WowInterface.ObjectManager.Player.IsInMeleeRange(WowInterface.ObjectManager.Target))
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if ((WowInterface.ObjectManager.Player.HealthPercentage < 20
                    && CastSpellIfPossibleRogue(cloakOfShadowsSpell, 0, true)))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if ((WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 16
                        && CastSpellIfPossibleRogue(sprintSpell, 0, true)))
                {
                    return;
                }
            }

            if (CastSpellIfPossibleRogue(eviscerateSpell, WowInterface.ObjectManager.TargetGuid, true, true, 5)
                || CastSpellIfPossibleRogue(mutilateSpell, WowInterface.ObjectManager.TargetGuid, true))
            {
                return;
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