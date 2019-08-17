using AmeisenBotX.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateLoadingScreen : State
    {
        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }

        public StateLoadingScreen(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
        }

        public override void Enter()
        {

        }

        public override void Execute()
        {
            if (ObjectManager.IsWorldLoaded)
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
        }

        public override void Exit()
        {

        }
    }
}
