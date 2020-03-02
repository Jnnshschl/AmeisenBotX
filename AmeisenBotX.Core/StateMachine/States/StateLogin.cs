using AmeisenBotX.Core.Common;
using System;
using System.Threading;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateLogin : BasicState
    {
        public StateLogin(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine)
        {
            Config = config;
            WowInterface = wowInterface;
        }

        private AmeisenBotConfig Config { get; }

        private WowInterface WowInterface { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            Thread.Sleep(1000);

            if (WowInterface.LoginHandler.Login(WowInterface.XMemory.Process, Config.Username, Config.Password, Config.CharacterSlot))
            {
                if (WowInterface.XMemory.Read(WowInterface.OffsetList.ChatOpened, out byte isChatOpened)
                    && isChatOpened == 0x1)
                {
                    // send enter to close the chat lmao
                    BotUtils.SendKey(WowInterface.XMemory.Process.MainWindowHandle, new IntPtr(0x0D));
                }
                else
                {
                    AmeisenBotStateMachine.SetState(BotState.Idle);
                }
            }
            else
            {
                WowInterface.XMemory.Process.Kill();
                AmeisenBotStateMachine.SetState(BotState.None);
            }
        }

        public override void Exit()
        {
        }
    }
}