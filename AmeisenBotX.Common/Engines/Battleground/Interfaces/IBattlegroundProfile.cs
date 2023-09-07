using AmeisenBotX.Common.BehaviorTree.Interfaces;

namespace AmeisenBotX.Common.Engines.Battleground.Interfaces
{
    public interface IBattlegroundProfile
    {
        IBlackboard JBgBlackboard { get; }

        void Execute();
    }
}