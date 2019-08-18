using AmeisenBotX.Core.Character;
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
        private ILoginHandler LoginHandler { get; }
        private AmeisenBotConfig Config { get; }
        private IOffsetList OffsetList { get; }
        private CharacterManager CharacterManager { get; }

        public StateLogin(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, CharacterManager characterManager) : base(stateMachine)
        {
            Config = config;
            OffsetList = offsetList;
            CharacterManager = characterManager;
            LoginHandler = new DefaultLoginHandler(AmeisenBotStateMachine.XMemory, offsetList);
        }

        public override void Enter()
        {

        }

        public override void Execute()
        {
            if (LoginHandler.Login(AmeisenBotStateMachine.XMemory.Process, Config.Username, Config.Password, Config.CharacterSlot))
            {
                if (AmeisenBotStateMachine.XMemory.Read(OffsetList.ChatOpened, out byte isChatOpened)
                    && isChatOpened == 0x1)
                    CharacterManager.SendKey(new IntPtr(0x0D)); // send enter to close the chat
                else
                    AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
            }
        }

        public override void Exit()
        {

        }
    }
}
