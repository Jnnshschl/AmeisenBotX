using System;
using System.Diagnostics;
using System.Threading;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateStartWow : BasicState
    {
        public StateStartWow(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        private DateTime WowStart { get; set; }

        public override void Enter()
        {
            WowInterface.WowProcess = Process.Start(Config.PathToWowExe);
            WowInterface.WowProcess.WaitForInputIdle();
            Thread.Sleep(1000);
            WowInterface.XMemory.Attach(WowInterface.WowProcess);
            WowStart = DateTime.Now;
        }

        public override void Execute()
        {
            if (DateTime.Now - WowStart > TimeSpan.FromSeconds(8) && WowInterface.WowProcess.HasExited)
            {
                StateMachine.SetState(BotState.None);
                return;
            }

            if (Config.AutoLogin)
            {
                StateMachine.SetState(BotState.Login);
            }
            else
            {
                StateMachine.SetState(BotState.Idle);
            }
        }

        public override void Exit()
        {
        }
    }
}