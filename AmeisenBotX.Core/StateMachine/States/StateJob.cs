using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.Jobs;
using AmeisenBotX.Core.Movement;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateJob : BasicState
    {
        public StateJob(AmeisenBotStateMachine stateMachine, JobEngine jobEngine) : base(stateMachine)
        {
            JobEngine = jobEngine;
        }

        private JobEngine JobEngine { get; }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            JobEngine.Execute();
        }

        public override void Exit()
        {
            JobEngine.Reset();
        }
    }
}