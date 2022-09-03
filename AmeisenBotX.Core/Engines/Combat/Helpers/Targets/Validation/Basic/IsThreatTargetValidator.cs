using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic
{
    public class IsInCombatTargetValidator : ITargetValidator
    {
        public bool IsValid(IWowUnit unit)
        {
            return unit.IsInCombat;
        }
    }
}