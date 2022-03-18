using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic
{
    public class IsAttackableTargetValidator : ITargetValidator
    {
        public IsAttackableTargetValidator(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        private AmeisenBotInterfaces Bot { get; }

        public bool IsValid(IWowUnit unit)
        {
            return Bot.Db.GetReaction(Bot.Player, unit)
                is WowUnitReaction.Hostile
                or WowUnitReaction.Neutral;
        }
    }
}