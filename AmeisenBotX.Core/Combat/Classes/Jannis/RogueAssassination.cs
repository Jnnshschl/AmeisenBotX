using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using System.Collections.Generic;
using static AmeisenBotX.Core.Utils.InterruptManager;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class RogueAssassination : BasicCombatClass
    {
        public RogueAssassination(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(sliceAndDiceSpell, () => TryCastSpellRogue(sliceAndDiceSpell, 0, true, true, 1)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(coldBloodSpell, () => TryCastSpellRogue(coldBloodSpell, 0, true)));

            InterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => TryCastSpellRogue(kickSpell, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Assasination Rogue spec.";

        public override string Displayname => "[WIP] Rogue Assasination";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicAgilityComparator(new List<WowArmorType>() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

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

        public override WowClass WowClass => WowClass.Rogue;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetManagerDps))
            {
                if ((WowInterface.ObjectManager.Player.HealthPercentage < 20
                        && TryCastSpellRogue(cloakOfShadowsSpell, 0, true)))
                {
                    return;
                }

                if (WowInterface.ObjectManager.Target != null)
                {
                    if ((WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 16
                            && TryCastSpellRogue(sprintSpell, 0, true)))
                    {
                        return;
                    }
                }

                if (TryCastSpellRogue(eviscerateSpell, WowInterface.ObjectManager.TargetGuid, true, true, 5)
                    || TryCastSpellRogue(mutilateSpell, WowInterface.ObjectManager.TargetGuid, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            base.OutOfCombatExecute();
        }
    }
}