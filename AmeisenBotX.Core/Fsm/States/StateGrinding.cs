namespace AmeisenBotX.Core.Fsm.States
{
    public class StateGrinding : BasicState
    {
        public StateGrinding(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot) : base(stateMachine, config, bot)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            Bot.Grinding.Execute();
        }

        public override void Leave()
        {
            Bot.Grinding.Exit();
        }
    }
}