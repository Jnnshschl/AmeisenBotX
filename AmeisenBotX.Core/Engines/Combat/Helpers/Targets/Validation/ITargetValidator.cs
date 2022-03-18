using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation
{
    public interface ITargetValidator
    {
        bool IsValid(IWowUnit unit);
    }
}