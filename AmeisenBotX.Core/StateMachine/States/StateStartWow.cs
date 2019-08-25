using AmeisenBotX.Memory;
using System.Diagnostics;
using System.Threading;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateStartWow : State
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
            Thread.Sleep(2000);

            WowProcess.WaitForInputIdle();
            XMemory.Attach(WowProcess);
        }

        public override void Execute()
        {
            if (WowProcess.HasExited)
            {
                AmeisenBotStateMachine.SetState(AmeisenBotState.None);
                return;
            }

            if (Config.AutoLogin)
                AmeisenBotStateMachine.SetState(AmeisenBotState.Login);
            else
                AmeisenBotStateMachine.SetState(AmeisenBotState.Idle);
        }

        public override void Exit()
        {
        }
    }
}