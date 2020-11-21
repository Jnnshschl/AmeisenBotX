using AmeisenBotX.Core.Statemachine.Enums;

namespace AmeisenBotX.Core.Tactic
{
    public interface ITactic
    {
        bool ExecuteTactic(CombatClassRole role, bool isMelee, out bool handlesMovement, out bool allowAttacking);
    }
}