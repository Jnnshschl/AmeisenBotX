using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Objects;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public class SmartMovementEngine : IMovementEngine
    {
        public SmartMovementEngine(WowInterface wowInterface, MovementSettings movementSettings)
        {
            WowInterface = wowInterface;
            MovementSettings = movementSettings;

            State = MovementEngineState.None;
            TryCount = 0;

            PlayerVehicle = new BasicVehicle(wowInterface, movementSettings.MaxSteering, movementSettings.MaxVelocity, movementSettings.MaxAcceleration);
        }

        public Queue<Vector3> CurrentPath { get; private set; }

        public Vector3 CurrentPathTargetPosition { get; private set; }

        public bool HasMoved { get; private set; }

        public DateTime LastJumpCheck { get; private set; }

        public DateTime LastLastPositionUpdate { get; private set; }

        public Vector3 LastPosition { get; private set; }

        public Vector3 LastTargetPosition { get; private set; }

        public MovementSettings MovementSettings { get; private set; }

        public IObjectManager ObjectManager { get; }

        public BasicVehicle PlayerVehicle { get; private set; }

        public MovementEngineState State { get; private set; }

        public Vector3 TargetPosition { get; private set; }

        public float TargetRotation { get; private set; }

        public int TryCount { get; private set; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if ((DateTime.Now - LastLastPositionUpdate > TimeSpan.FromMilliseconds(1000) && LastPosition.GetDistance(WowInterface.ObjectManager.Player.Position) > 24) || TryCount > 2)
            {
                Reset();
                return;
            }

            if (CurrentPath.Count == 0)
            {
                TryCount = 0;
                List<Vector3> nodes = WowInterface.PathfindingHandler.GetPath((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, TargetPosition);

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
            Vector3 currentPosition = WowInterface.ObjectManager.Player.Position;
            Vector3 targetPosition = CurrentPath.Peek();
            double distanceToTargetPosition = currentPosition.GetDistance(targetPosition);

            if (distanceToTargetPosition > 512)
            {
                Reset();
                return;
            }
            else if (distanceToTargetPosition < MovementSettings.WaypointCheckThreshold)
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

            Vector3 positionToGoTo = BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Rotation, targetPosition, 2);
            bool updateForces = true;

            switch (State)
            {
                case MovementEngineState.Moving:
                    forces.Add(PlayerVehicle.Seek(positionToGoTo, 1));
                    break;

                case MovementEngineState.DirectMoving:
                    WowInterface.CharacterManager.MoveToPosition(targetPosition);
                    updateForces = false;
                    break;

                case MovementEngineState.Following:
                    forces.Add(PlayerVehicle.Seek(positionToGoTo, 1));
                    forces.Add(PlayerVehicle.Seperate(0.5f));
                    break;

                case MovementEngineState.Chasing:
                    forces.Add(PlayerVehicle.Seek(positionToGoTo, 1));
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

            if (updateForces)
            {
                PlayerVehicle.Update(forces);
            }

            if (DateTime.Now - LastJumpCheck > TimeSpan.FromMilliseconds(250))
            {
                double distanceTraveled = LastPosition.GetDistance(WowInterface.ObjectManager.Player.Position);
                if ((LastPosition.X == 0 && LastPosition.Y == 0 && LastPosition.Z == 0) || distanceTraveled < 0.01)
                {
                    ++TryCount;
                }

                if (TryCount > 0)
                {
                    WowInterface.CharacterManager.Jump();
                    TryCount = 0;
                }

                LastPosition = WowInterface.ObjectManager.Player.Position;
                LastLastPositionUpdate = DateTime.Now;
                LastJumpCheck = DateTime.Now;
            }

            // if the target position is higher than us, jump
            if (distanceToTargetPosition < 4 && currentPosition.Z + 3 < targetPosition.Z)
            {
                WowInterface.CharacterManager.Jump();
            }

            LastTargetPosition = WowInterface.ObjectManager.Player.Position;
            HasMoved = true;
        }

        public void Reset()
        {
            State = MovementEngineState.None;
            CurrentPath = new Queue<Vector3>();
            HasMoved = false;
            TryCount = 0;
            LastLastPositionUpdate = DateTime.Now;
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
    }
}