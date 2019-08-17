using AmeisenBotX.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    class StateAttacking : State
    {
        private AmeisenBotConfig Config { get; }

        private ObjectManager ObjectManager { get; }

        public StateAttacking(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
        }

        public override void Enter()
        {

        }

        public override void Execute()
        {

        }

        public override void Exit()
        {

        }
    }
}
