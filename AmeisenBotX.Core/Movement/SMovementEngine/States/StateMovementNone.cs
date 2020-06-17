using AmeisenBotX.Core.Movement.SMovementEngine.Enums;

namespace AmeisenBotX.Core.Movement.SMovementEngine.States
{
    public class StateMovementNone : BasicMovementState
    {
        public StateMovementNone(StateBasedMovementEngine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
        }

        public override void Execute()
        {
            if (StateMachine.VehicleTargetPosition != default
                 && StateMachine.FinalTargetPosition != default
                 && !StateMachine.IsAtTargetPosition)
            {
                StateMachine.SetState((int)MovementState.Pathfinding);
            }
        }

        public override void Exit()
        {
        }
    }
}