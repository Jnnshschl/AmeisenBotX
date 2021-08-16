using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Logic
{
    public interface IAmeisenBotLogic
    {
        event Action OnWoWStarted;

        void Tick();
    }
}
