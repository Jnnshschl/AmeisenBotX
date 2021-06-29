using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;

namespace AmeisenBotX.Core.Combat.Classes.Jannis
{
    public class RogueAssassination : BasicCombatClass
    {
        public RogueAssassination(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(sliceAndDiceSpell, () => TryCastSpellRogue(sliceAndDiceSpell, 0, true, true, 1)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(coldBloodSpell, () => TryCastSpellRogue(coldBloodSpell, 0, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellRogue(kickSpell, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Assasination Rogue spec.";

        public override string Displayname => "[WIP] Rogue Assasination";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicAgilityComparator(new() { WowArmorType.SHIELDS });

        public override WowRole Role => WowRole.Dps;

        public override TalentTree Talents { get; } = new()
        {
            Tree1 = new()
            {
                { 3, new(1, 3, 2) },
                { 4, new(1, 4, 5) },
                { 5, new(1, 5, 2) },
                { 6, new(1, 6, 3) },
                { 9, new(1, 9, 5) },
                { 10, new(1, 10, 3) },
                { 11, new(1, 11, 5) },
                { 13, new(1, 13, 1) },
                { 16, new(1, 16, 5) },
                { 17, new(1, 17, 2) },
                { 19, new(1, 19, 1) },
                { 21, new(1, 21, 3) },
                { 22, new(1, 22, 3) },
                { 23, new(1, 23, 3) },
                { 24, new(1, 24, 1) },
                { 26, new(1, 26, 5) },
                { 27, new(1, 27, 1) },
            },
            Tree2 = new()
            {
                { 3, new(2, 3, 5) },
                { 6, new(2, 6, 5) },
                { 9, new(2, 9, 3) },
            },
            Tree3 = new()
            {
                { 1, new(3, 1, 5) },
                { 3, new(3, 3, 2) },
            },
        };

        public override bool UseAutoAttacks => true;

        public override string Version => "1.0";

        public override bool WalkBehindEnemy => true;

        public override WowClass WowClass => WowClass.Rogue;

        public override void Execute()
        {
            base.Execute();

            if (SelectTarget(TargetProviderDps))
            {
                if ((WowInterface.Player.HealthPercentage < 20
                        && TryCastSpellRogue(cloakOfShadowsSpell, 0, true)))
                {
                    return;
                }

                if (WowInterface.Target != null)
                {
                    if ((WowInterface.Target.Position.GetDistance(WowInterface.Player.Position) > 16
                            && TryCastSpellRogue(sprintSpell, 0, true)))
                    {
                        return;
                    }
                }

                if (TryCastSpellRogue(eviscerateSpell, WowInterface.Target.Guid, true, true, 5)
                    || TryCastSpellRogue(mutilateSpell, WowInterface.Target.Guid, true))
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