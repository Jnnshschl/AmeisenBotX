using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Tactic
{
    public interface ITacticEngine
    {
        bool Execute(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking);

        bool HasTactics();

        void LoadTactics(params ITactic[] tactics);

        void Reset();
    }
}