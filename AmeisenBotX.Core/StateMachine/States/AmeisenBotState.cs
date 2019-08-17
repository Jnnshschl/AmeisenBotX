using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public enum AmeisenBotState
    {
        None,
        StartWow,
        Login,
        Idle,
        Following,
        Attacking,
        Healing,
        LoadingScreen
    }
}
