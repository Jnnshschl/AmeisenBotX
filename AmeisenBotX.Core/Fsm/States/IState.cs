namespace AmeisenBotX.Statemachine.States
{
    public interface IState
    {
        /// <summary>
        /// Will be called once, when we enter the state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Will be polled when the state is active.
        /// </summary>
        void Execute();

        /// <summary>
        /// Will be called once, when we leave the state.
        /// </summary>
        void Leave();
    }
}