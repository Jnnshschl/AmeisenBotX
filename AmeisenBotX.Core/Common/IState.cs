namespace AmeisenBotX.Core.Common
{
    public interface IState
    {
        void Enter();

        void Execute();

        void Exit();
    }
}