using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateStartWow : State
    {
        private AmeisenBotConfig Config { get; }
        private Process WowProcess { get; set; }
        private XMemory XMemory { get; }

        public StateStartWow(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, Process wowProcess, XMemory xMemory) : base(stateMachine)
        {
            Config = config;
            WowProcess = wowProcess;
            XMemory = xMemory;
        }

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
