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
            if (StateMachine.TargetPosition != default && !StateMachine.IsAtTargetPosition)
            {
                double zDiff = StateMachine.TargetPosition.Z - WowInterface.ObjectManager.Player.Position.Z;

                if (zDiff > 2)
                {
                    // target position is above us
                }
                else if (zDiff < -2)
                {
                    // target position is below us
                }
                else
                {
                    // target is on our level
                }

                StateMachine.SetState((int)MovementState.Pathfinding);
            }
        }

        public override void Exit()
        {
        }
    }
}