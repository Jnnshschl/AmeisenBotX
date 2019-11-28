using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Objects;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Pathfinding.Objects;
using static AmeisenBotX.Core.Movement.Objects.BasicVehicle;

namespace AmeisenBotX.Core.Movement
{
    public class SmartMovementEngine : IMovementEngine
    {
        public delegate List<Vector3> GeneratePathFunction(Vector3 start, Vector3 end);

        public SmartMovementEngine(GetPositionFunction getPositionFunction, GetRotationFunction getRotationFunction, MoveToPositionFunction moveToPositionFunction, GeneratePathFunction generatePathFunction, JumpFunction jumpFunction, ObjectManager objectManager, MovementSettings movementSettings)
        {
            State = MovementEngineState.None;
            GetPosition = getPositionFunction;
            GetRotation = getRotationFunction;
            MoveToPosition = moveToPositionFunction;
            GeneratePath = generatePathFunction;
            MovementSettings = movementSettings;
            ObjectManager = objectManager;
            Jump = jumpFunction;

            PlayerVehicle = new BasicVehicle(getPositionFunction, getRotationFunction, moveToPositionFunction, jumpFunction, objectManager, movementSettings.MaxSteering, movementSettings.MaxVelocity, movementSettings.MaxAcceleration);
        }

        public MovementEngineState State { get; private set; }

        public Queue<Vector3> CurrentPath { get; private set; }

        public Vector3 TargetPosition { get; private set; }

        public Vector3 CurrentPathTargetPosition { get; private set; }

        public Vector3 LastPosition { get; private set; }

        public DateTime LastJumpCheck { get; private set; }

        public bool HasMoved { get; private set; }

        public float TargetRotation { get; private set; }

        public ObjectManager ObjectManager { get; }

        public JumpFunction Jump { get; set; }

        public MoveToPositionFunction MoveToPosition { get; set; }

        public GetRotationFunction GetRotation { get; set; }

        public GetPositionFunction GetPosition { get; set; }

        public GeneratePathFunction GeneratePath { get; set; }

        public BasicVehicle PlayerVehicle { get; private set; }

        public MovementSettings MovementSettings { get; private set; }

        public void Execute()
        {
            if (CurrentPath.Count == 0 || CurrentPathTargetPosition.GetDistance(TargetPosition) > 1)
            {
                List<Vector3> nodes = GeneratePath.Invoke(GetPosition.Invoke(), TargetPosition);

                if (nodes.Count == 0)
                {
                    // pathfinding was unsuccessful
                    return;
                }

                foreach (Vector3 node in nodes)
                {
                    CurrentPath.Enqueue(node);
                }

                CurrentPathTargetPosition = TargetPosition;
            }

            List<Vector3> forces = new List<Vector3>();
            Vector3 currentPosition = GetPosition.Invoke();
            Vector3 targetPosition = CurrentPath.Peek();
            double distanceToTargetPosition = currentPosition.GetDistance2D(targetPosition);

            if (distanceToTargetPosition < MovementSettings.WaypointCheckThreshold)
            {
                if (CurrentPath.Count > 0)
                {
                    targetPosition = CurrentPath.Dequeue();
                }
                else if (CurrentPath.Count == 0)
                {
                    return;
                }
            }

            Vector3 positionToGoTo = MoveAhead(targetPosition, 1.5);

            switch (State)
            {
                case MovementEngineState.Moving:
                    forces.Add(PlayerVehicle.Seek(positionToGoTo, 1));
                    break;

                case MovementEngineState.Following:
                    forces.Add(PlayerVehicle.Seek(positionToGoTo, 1));
                    forces.Add(PlayerVehicle.Seperate(1));
                    break;

                case MovementEngineState.Chasing:
                    forces.Add(PlayerVehicle.Pursuit(positionToGoTo, 1, TargetRotation));
                    break;

                case MovementEngineState.Fleeing:
                    forces.Add(PlayerVehicle.Flee(positionToGoTo, 1));
                    break;

                case MovementEngineState.Evading:
                    forces.Add(PlayerVehicle.Evade(positionToGoTo, 1, TargetRotation));
                    break;

                case MovementEngineState.Wandering:
                    forces.Add(PlayerVehicle.Wander(1));
                    break;

                case MovementEngineState.Stuck:
                    forces.Add(PlayerVehicle.Unstuck(1));
                    break;

                default:
                    return;
            }

            // move
            PlayerVehicle.Update(forces);

            if (DateTime.Now - LastJumpCheck > TimeSpan.FromMilliseconds(250))
            {
                double distanceTraveled = LastPosition.GetDistance(GetPosition.Invoke());
                if ((LastPosition.X == 0 && LastPosition.Y == 0 && LastPosition.Z == 0) || distanceTraveled < 0.3)
                {
                    Jump.Invoke();
                }

                LastPosition = GetPosition.Invoke();
                LastJumpCheck = DateTime.Now;
            }

            HasMoved = true;
        }

        private Vector3 MoveAhead(Vector3 targetPosition, double offset)
        {
            float rotation = GetRotation.Invoke();
            double x = targetPosition.X + (Math.Cos(rotation) * offset);
            double y = targetPosition.Y + (Math.Sin(rotation) * offset);

            return new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = targetPosition.Z
            };
        }

        public void SetState(MovementEngineState state, Vector3 position, float targetRotation = 0f)
        {
            if (State != state)
            {
                Reset();
                State = state;
            }

            TargetPosition = position;
            TargetRotation = targetRotation;
        }

        private void Reset()
        {
            State = MovementEngineState.None;
            CurrentPath = new Queue<Vector3>(); 
            HasMoved = false;
        }
    }
}
