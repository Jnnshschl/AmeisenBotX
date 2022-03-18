using AmeisenBotX.Wow.Objects;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Priority
{
    public interface ITargetPrioritizer
    {
        bool HasPriority(IWowUnit unit);
    }
}