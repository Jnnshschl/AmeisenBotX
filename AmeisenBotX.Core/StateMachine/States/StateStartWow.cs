using AmeisenBotX.Memory;
using System.Diagnostics;
using System.Threading;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateStartWow : BasicState
    {
        public StateStartWow(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, Process wowProcess, XMemory xMemory) : base(stateMachine)
        {
            Config = config;
            WowProcess = wowProcess;
            XMemory = xMemory;
        }

        private AmeisenBotConfig Config { get; }

        private Process WowProcess { get; set; }

        private XMemory XMemory { get; }

        public override void Enter()
        {
            WowProcess = Process.Start(Config.PathToWowExe);
            WowProcess.WaitForInputIdle();
            Thread.Sleep(1000);
            XMemory.Attach(WowProcess);
        }

        public override void Execute()
        {
            if (WowProcess.HasExited)
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