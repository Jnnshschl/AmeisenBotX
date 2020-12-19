using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Quest.Objects.Quests
{
    public interface IBotQuest
    {
        void AcceptQuest();
        bool CompleteQuest();
        void Execute();
        string Name { get; }
        bool Accepted { get; }
        bool Finished { get; }
        bool Returned { get; }
        int Id { get; }
    }
}
