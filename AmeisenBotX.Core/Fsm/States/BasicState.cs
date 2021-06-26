using AmeisenBotX.Statemachine.States;

namespace AmeisenBotX.Core.Fsm.States
{
    /// <summary>
    /// Basic form of a simple state, containing all common classes (StateMachine, Config, WowInterface) used by the bot.
    /// </summary>
    public abstract class BasicState : IState
    {
        public BasicState(AmeisenBotFsm stateMachine, AmeisenBotConfig config, WowInterface wowInterface)
        {
            StateMachine = stateMachine;
            Config = config;
            WowInterface = wowInterface;
        }

        internal AmeisenBotConfig Config { get; }

        internal AmeisenBotFsm StateMachine { get; }

        internal WowInterface WowInterface { get; }

        ///<inheritdoc cref="IState.Enter"/>
        public abstract void Enter();

        ///<inheritdoc cref="IState.Execute"/>
        public abstract void Execute();

        ///<inheritdoc cref="IState.Leave"/>
        public abstract void Leave();
    }
}