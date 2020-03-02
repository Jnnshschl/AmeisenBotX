using System.Diagnostics;
using System.Threading;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateStartWow : BasicState
    {
        public StateStartWow(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine)
        {
            Config = config;
            WowInterface = wowInterface;
        }

        private AmeisenBotConfig Config { get; }

        private WowInterface WowInterface { get; set; }

        public override void Enter()
        {
            WowInterface.WowProcess = Process.Start(Config.PathToWowExe);
            WowInterface.WowProcess.WaitForInputIdle();
            Thread.Sleep(1000);
            WowInterface.XMemory.Attach(WowInterface.WowProcess);
        }

        public override void Execute()
        {
            if (WowInterface.WowProcess.HasExited)
            {
                AmeisenBotStateMachine.SetState(BotState.None);
                return;
            }

            if (Config.AutoLogin)
            {
                AmeisenBotStateMachine.SetState(BotState.Login);
            }
            else
            {
                AmeisenBotStateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}