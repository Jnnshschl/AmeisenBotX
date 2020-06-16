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

            PlayerVehicle.OnMoveCharacter += (x) => VehicleTargetPosition = x;
        }

        public override event StateMachineTick OnStateMachineTick;

        public override event StateMachineOverride OnStateOverride;

        public Vector3 FinalTargetPosition { get; private set; }

        public bool IsAtTargetPosition => FinalTargetPosition != default && FinalTargetPosition.GetDistanceIgnoreZ(WowInterface.ObjectManager.Player.Position) < MovementSettings.WaypointCheckThreshold;

        public Vector3 LastPlayerPosition { get; private set; }

        public MovementAction MovementAction { get; private set; }

        public Queue<Vector3> Nodes { get; private set; }

        public List<Vector3> Path => Nodes.ToList();

        public Vector3 VehicleTargetPosition { get; private set; }

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
            FinalTargetPosition = Vector3.Zero;
            VehicleTargetPosition = Vector3.Zero;
            Nodes.Clear();
        }

        public void SetMovementAction(MovementAction movementAction, Vector3 positionToGoTo, float targetRotation = 0f)
        {
            MovementAction = movementAction;
            LastPlayerPosition = WowInterface.ObjectManager.Player.Position;

            PlayerVehicle.Update(GetForces(positionToGoTo, targetRotation));

            if (movementAction == MovementAction.Fleeing
                || movementAction == MovementAction.Wandering
                || movementAction == MovementAction.Chasing
                || movementAction == MovementAction.Evading
                || movementAction == MovementAction.Unstuck)
            {
                // we dont care about reaching the target
                FinalTargetPosition = VehicleTargetPosition;
            }
            else
            {
                FinalTargetPosition = positionToGoTo;
            }
        }

        private List<Vector3> GetForces(Vector3 targetPosition, float rotation = 0f)
        {
            List<Vector3> forces = new List<Vector3>();

            switch (MovementAction)
            {
                case MovementAction.Moving:
                    forces.Add(PlayerVehicle.Seek(targetPosition, 1f));
                    forces.Add(PlayerVehicle.AvoidObstacles(2f));
                    break;

                case MovementAction.Following:
                    forces.Add(PlayerVehicle.Seek(targetPosition, 1f));
                    forces.Add(PlayerVehicle.Seperate(1f));
                    forces.Add(PlayerVehicle.AvoidObstacles(2f));
                    break;

                case MovementAction.Chasing:
                    forces.Add(PlayerVehicle.Seek(targetPosition, 1f));
                    break;

                case MovementAction.Fleeing:
                    Vector3 fleeForce = PlayerVehicle.Flee(targetPosition, 1f);
                    fleeForce.Z = 0; // set z to zero to avoid going under the terrain
                    forces.Add(fleeForce);
                    break;

                case MovementAction.Evading:
                    forces.Add(PlayerVehicle.Evade(targetPosition, 1f, rotation));
                    break;

                case MovementAction.Wandering:
                    Vector3 wanderForce = PlayerVehicle.Wander(1f);
                    wanderForce.Z = 0; // set z to zero to avoid going under the terrain
                    forces.Add(wanderForce);
                    break;

                case MovementAction.Unstuck:
                    forces.Add(PlayerVehicle.Unstuck(1f));
                    break;

                default:
                    break;
            }

            return forces;
        }
    }
}