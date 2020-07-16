namespace AmeisenBotX.Core.Battleground.Jannis.Profiles
{
    public interface IBattlegroundProfile
    {
        CtfBlackboard JBgBlackboard { get; }

        void Execute();
    }
}