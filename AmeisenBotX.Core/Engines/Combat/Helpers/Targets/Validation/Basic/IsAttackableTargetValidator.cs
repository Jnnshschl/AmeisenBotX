using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic
{
    public class IsAttackableTargetValidator(AmeisenBotInterfaces bot) : ITargetValidator
    {
        private AmeisenBotInterfaces Bot { get; } = bot;

        public bool IsValid(IWowUnit unit)
        {
            return Bot.Db.GetReaction(Bot.Player, unit)
                is WowUnitReaction.Hostile
                or WowUnitReaction.Neutral;
        }
    }
}