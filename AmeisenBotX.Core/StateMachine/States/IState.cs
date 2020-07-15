namespace AmeisenBotX.Statemachine.States
{
    public interface IState
    {
        void Enter();

        void Execute();

        void Exit();
    }
}