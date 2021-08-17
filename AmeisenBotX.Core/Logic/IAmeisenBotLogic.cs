using System;

namespace AmeisenBotX.Core.Logic
{
    public interface IAmeisenBotLogic
    {
        event Action OnWoWStarted;

        void Tick();
    }
}