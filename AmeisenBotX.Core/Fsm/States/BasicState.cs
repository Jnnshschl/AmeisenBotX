using AmeisenBotX.Statemachine.States;

namespace AmeisenBotX.Core.Fsm.States
{
    /// <summary>
    /// Basic form of a simple state, containing all common classes (StateMachine, Config, Bot) used by the bot.
    /// </summary>
    public abstract class BasicState : IState
    {
        public BasicState(AmeisenBotFsm stateMachine, AmeisenBotConfig config, AmeisenBotInterfaces bot)
        {
            StateMachine = stateMachine;
            Config = config;
            Bot = bot;
        }

        internal AmeisenBotConfig Config { get; }

        internal AmeisenBotFsm StateMachine { get; }

        internal AmeisenBotInterfaces Bot { get; }

        ///<inheritdoc cref="IState.Enter"/>
        public abstract void Enter();

        ///<inheritdoc cref="IState.Execute"/>
        public abstract void Execute();

        ///<inheritdoc cref="IState.Leave"/>
        public abstract void Leave();
    }
}