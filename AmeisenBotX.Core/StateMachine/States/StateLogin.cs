using AmeisenBotX.Core.LoginHandler;
using AmeisenBotX.Core.OffsetLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateLogin : State
    {
        private ILoginHandler LoginHandler { get; set; }
        private AmeisenBotConfig Config { get; }

        public StateLogin(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList) : base(stateMachine)
        {
            Config = config;
            LoginHandler = new DefaultLoginHandler(AmeisenBotStateMachine.XMemory, offsetList);
        }

        public override void Enter()
        {

        }

        public override void Execute()
        {
            if(LoginHandler.Login(AmeisenBotStateMachine.XMemory.Process, Config.Username, Config.Password, Config.CharacterSlot))
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
            }
        }

        public override void Exit()
        {

        }
    }
}
