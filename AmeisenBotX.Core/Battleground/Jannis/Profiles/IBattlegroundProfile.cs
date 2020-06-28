namespace AmeisenBotX.Core.Battleground.Jannis.Profiles
{
    public interface IBattlegroundProfile
    {
        JBgBlackboard JBgBlackboard { get; }

        void Execute();
    }
}