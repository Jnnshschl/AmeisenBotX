using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets.Validation.Basic
{
    public class IsValidAliveTargetValidator : ITargetValidator
    {
        public bool IsValid(IWowUnit unit)
        {
            // unit should be alive, attackable and no critter
            return IWowUnit.IsValidAlive(unit)
                && unit.ReadType() is not WowCreatureType.Critter;
        }
    }
}