using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateNone : State
    {
        private AmeisenBotConfig Config { get; }
        private AmeisenBotState EntryState { get; set; }

        public StateNone(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config) : base(stateMachine)
        {
            Config = config;
        }

        public override void Enter()
        {
            if (Config.AutostartWow)
            {
                EntryState = AmeisenBotState.StartWow;
            }
            else if (Config.AutoLogin)
            {
                EntryState = AmeisenBotState.Login;
            }
            else
                EntryState = AmeisenBotState.Idle;
        }

        public override void Execute()
        {
            AmeisenBotStateMachine.SetState(EntryState);
        }

        public override void Exit()
        {

        }
    }
}
