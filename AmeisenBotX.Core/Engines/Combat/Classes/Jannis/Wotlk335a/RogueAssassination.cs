using AmeisenBotX.Core.Engines.Combat.Helpers.Aura.Objects;
using AmeisenBotX.Core.Managers.Character.Comparators;
using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Wow335a.Constants;

namespace AmeisenBotX.Core.Engines.Combat.Classes.Jannis.Wotlk335a
{
    public class RogueAssassination : BasicCombatClass
    {
        public RogueAssassination(AmeisenBotInterfaces bot) : base(bot)
        {
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Rogue335a.SliceAndDice, () => TryCastSpellRogue(Rogue335a.SliceAndDice, 0, true, true, 1)));
            MyAuraManager.Jobs.Add(new KeepActiveAuraJob(bot.Db, Rogue335a.ColdBlood, () => TryCastSpellRogue(Rogue335a.ColdBlood, 0, true)));

            InterruptManager.InterruptSpells = new()
            {
                { 0, (x) => TryCastSpellRogue(Rogue335a.Kick, x.Guid, true) }
            };
        }

        public override string Description => "FCFS based CombatClass for the Assasination Rogue spec.";

        public override string DisplayName2 => "[WIP] Rogue Assasination";

        public override bool HandlesMovement => false;

        public override bool IsMelee => true;

        public override IItemComparator ItemComparator { get; set; } = new BasicAgilityComparator(new() { WowArmorType.Shield });

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

        public override WowVersion WowVersion => WowVersion.WotLK335a;

        public override void Execute()
        {
            base.Execute();

            if (TryFindTarget(TargetProviderDps, out _))
            {
                if ((Bot.Player.HealthPercentage < 20
                        && TryCastSpellRogue(Rogue335a.CloakOfShadows, 0, true)))
                {
                    return;
                }

                if (Bot.Target != null)
                {
                    if ((Bot.Target.Position.GetDistance(Bot.Player.Position) > 16
                            && TryCastSpellRogue(Rogue335a.Sprint, 0, true)))
                    {
                        return;
                    }
                }

                if (TryCastSpellRogue(Rogue335a.Eviscerate, Bot.Wow.TargetGuid, true, true, 5)
                    || TryCastSpellRogue(Rogue335a.Mutilate, Bot.Wow.TargetGuid, true))
                {
                    return;
                }
            }
        }
    }
}