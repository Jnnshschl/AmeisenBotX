namespace AmeisenBotX.Core.Logic.States
{
    public class StateJob : BasicState
    {
        public StateJob(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public override void Enter()
        {
            Bot.Jobs.Enter();
        }

        public override void Execute()
        {
            Bot.Jobs.Execute();
        }

        public override void Leave()
        {
            Bot.Jobs.Reset();
        }
    }
}