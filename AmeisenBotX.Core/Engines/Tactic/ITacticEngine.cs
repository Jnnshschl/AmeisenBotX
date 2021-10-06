namespace AmeisenBotX.Core.Engines.Tactic
{
    public interface ITacticEngine
    {
        bool AllowAttacking { get; }

        bool PreventMovement { get; }

        bool Execute();
    }
}