namespace AmeisenBotX.Core.Engines.Battleground.Jannis.Profiles
{
    public interface IBattlegroundProfile
    {
        CtfBlackboard JBgBlackboard { get; }

        void Execute();
    }
}