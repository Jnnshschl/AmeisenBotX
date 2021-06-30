namespace AmeisenBotX.Core.Fsm.States
{
    public class StateJob : BasicState
    {
        public StateJob(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public override void Enter()
        {
            Bot.Jobs.Enter();
            Bot.Globals.IgnoreMountDistance = true;
        }

        public override void Execute()
        {
            Bot.Jobs.Execute();
        }

        public override void Leave()
        {
            Bot.Globals.IgnoreMountDistance = false;
            Bot.Jobs.Reset();
        }
    }
}