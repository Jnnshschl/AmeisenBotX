using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Objects;
using AmeisenBotX.Core.Movement.Pathfinding;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using AmeisenBotX.Core.Movement.SMovementEngine.States;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Movement.SMovementEngine
{
    public class StateBasedMovementEngine : AbstractStateMachine<BasicMovementState>, IMovementEngine
    {
        public StateBasedMovementEngine(WowInterface wowInterface, AmeisenBotConfig config, MovementSettings movementSettings, IPathfindingHandler pathfindingHandler)
        {
            WowInterface = wowInterface;
            PathfindingHandler = pathfindingHandler;
            MovementSettings = movementSettings;

            Nodes = new Queue<Vector3>();
            PlayerVehicle = new BasicVehicle(wowInterface, movementSettings.MaxSteering, movementSettings.MaxVelocity, movementSettings.MaxAcceleration);

            LastState = (int)MovementState.None;

            States = new Dictionary<int, BasicMovementState>()
            {
                { (int)MovementState.None, new StateMovementNone(this, config, WowInterface) },
                { (int)MovementState.Pathfinding, new StateMovementPathfinding(this, config, WowInterface) },
                { (int)MovementState.MoveToNode, new StateMovementMoveToNode(this, config, WowInterface) },
            };

            CurrentState = States.First();
            CurrentState.Value.Enter();
        }

        public override event StateMachineTick OnStateMachineTick;

        public override event StateMachineOverride OnStateOverride;

        public bool IsAtTargetPosition => TargetPosition != default && TargetPosition.GetDistance(WowInterface.ObjectManager.Player.Position) < MovementSettings.WaypointCheckThreshold;

        public Vector3 LastPlayerPosition { get; private set; }

        public MovementAction MovementAction { get; private set; }

        public Queue<Vector3> Nodes { get; private set; }

        public List<Vector3> Path => Nodes.ToList();

        public Vector3 TargetPosition { get; private set; }

        public float TargetRotation { get; private set; }

        internal MovementSettings MovementSettings { get; }

        internal IPathfindingHandler PathfindingHandler { get; }

        internal BasicVehicle PlayerVehicle { get; }

        internal WowInterface WowInterface { get; }

        public override void Execute()
        {
            CurrentState.Value.Execute();
            OnStateMachineTick?.Invoke();
        }

        public void Reset()
        {
            MovementAction = MovementAction.None;
            CurrentState = States.First(e => e.Key == (int)MovementState.None);
            TargetPosition = Vector3.Zero;
            Nodes.Clear();
        }

        public void SetMovementAction(MovementAction movementAction, Vector3 positionToGoTo, float targetRotation = 0f)
        {
            TargetRotation = targetRotation;

            if (TargetPosition.GetDistance(positionToGoTo) > 2.0 || MovementAction != movementAction)
            {
                MovementAction = movementAction;
                LastPlayerPosition = WowInterface.ObjectManager.Player.Position;
                TargetPosition = positionToGoTo;
            }
        }
    }
}