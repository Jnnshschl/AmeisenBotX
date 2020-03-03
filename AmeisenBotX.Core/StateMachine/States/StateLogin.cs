using AmeisenBotX.Core.Common;
using System;
using System.Threading;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateLogin : BasicState
    {
        public StateLogin(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

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
                    StateMachine.SetState(BotState.Idle);
                }
            }
            else
            {
                WowInterface.XMemory.Process.Kill();
                StateMachine.SetState(BotState.None);
            }
        }

        public override void Exit()
        {
        }
    }
}