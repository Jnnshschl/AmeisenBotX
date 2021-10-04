using AmeisenBotX.Core.Engines.Jobs.Profiles;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Jobs
{
    public interface IJobEngine
    {
        List<ulong> NodeBlacklist { get; set; }

        IJobProfile Profile { get; set; }

        void Enter();

        void Execute();

        void Reset();
    }
}