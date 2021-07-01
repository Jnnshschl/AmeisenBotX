using AmeisenBotX.Core.Jobs.Profiles;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Jobs
{
    public interface IJobEngine
    {
        bool GeneratedPathToNode { get; }

        List<ulong> NodeBlacklist { get; set; }

        IJobProfile Profile { get; set; }

        void Enter();

        void Execute();

        void Reset();
    }
}