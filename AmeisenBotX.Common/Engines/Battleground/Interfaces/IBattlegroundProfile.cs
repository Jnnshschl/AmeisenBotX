using AmeisenBotX.Common.BehaviorTree.Interfaces;
using AmeisenBotX.Core.Engines.Battleground.Jannis;

namespace AmeisenBotX.Common.Engines.Battleground.Interfaces
{
    public interface IBattlegroundProfile
    {
        ICtfBlackboard JBgBlackboard { get; }

        void Execute();
    }
}