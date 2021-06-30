namespace AmeisenBotX.Core.Fsm.States
{
    public class StateQuesting : BasicState
    {
        public StateQuesting(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public override void Enter()
        {
            Bot.Quest.Start();
        }

        public override void Execute()
        {
            Bot.Quest.Execute();
        }

        public override void Leave()
        {
        }
    }
}