using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Engines.Tactic
{
    public interface ITacticEngine
    {
        bool Execute(out bool preventMovement, out bool allowAttacking);

        bool HasTactics();

        void Reset();
    }
}