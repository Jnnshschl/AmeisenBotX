namespace AmeisenBotX.Core.Fsm.States
{
    public class StateQuesting : BasicState
    {
        public StateQuesting(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
            WowInterface.QuestEngine.Start();
        }

        public override void Execute()
        {
            WowInterface.QuestEngine.Execute();
        }

        public override void Leave()
        {
        }
    }
}