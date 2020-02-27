using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.LoginHandler;
using AmeisenBotX.Core.OffsetLists;
using System;
using System.Threading;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateLogin : BasicState
    {
        public StateLogin(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, IOffsetList offsetList, CharacterManager characterManager) : base(stateMachine)
        {
            Config = config;
            OffsetList = offsetList;
            CharacterManager = characterManager;
            LoginHandler = new DefaultLoginHandler(AmeisenBotStateMachine.XMemory, offsetList);
        }

        private CharacterManager CharacterManager { get; }

        private AmeisenBotConfig Config { get; }

        private ILoginHandler LoginHandler { get; }

        private IOffsetList OffsetList { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            Thread.Sleep(1000);

            if (LoginHandler.Login(AmeisenBotStateMachine.XMemory.Process, Config.Username, Config.Password, Config.CharacterSlot))
            {
                if (AmeisenBotStateMachine.XMemory.Read(OffsetList.ChatOpened, out byte isChatOpened)
                    && isChatOpened == 0x1)
                {
                    // send enter to close the chat lmao
                    BotUtils.SendKey(AmeisenBotStateMachine.XMemory.Process.MainWindowHandle, new IntPtr(0x0D));
                }
                else
                {
                    AmeisenBotStateMachine.SetState(BotState.Idle);
                }
            }
            else
            {
                AmeisenBotStateMachine.XMemory.Process.Kill();
                AmeisenBotStateMachine.SetState(BotState.None);
            }
        }

        public override void Exit()
        {
        }
    }
}