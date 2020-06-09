using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Movement.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Movement
{
    public class SmartMovementEngine : IMovementEngine
    {
        public SmartMovementEngine(WowInterface wowInterface, MovementSettings movementSettings)
        {
            WowInterface = wowInterface;
            MovementSettings = movementSettings;

            Rnd = new Random();
            CurrentPath = new Queue<Vector3>();

            MovementAction = MovementAction.None;
            TryCount = 0;

            PlayerVehicle = new BasicVehicle(wowInterface, movementSettings.MaxSteering, movementSettings.MaxVelocity, movementSettings.MaxAcceleration);
        }

        public bool BurstCheckDistance { get; private set; }

        public Queue<Vector3> CurrentPath { get; private set; }

        public Vector3 CurrentPathTargetPosition { get; private set; }

        public bool HasMoved { get; private set; }

        public DateTime LastJumpCheck { get; private set; }

        public DateTime LastLastPositionUpdate { get; private set; }

        public Vector3 LastPosition { get; private set; }

        public Vector3 LastTargetPosition { get; private set; }

        public MovementAction MovementAction { get; private set; }

        public MovementSettings MovementSettings { get; private set; }

        public IObjectManager ObjectManager { get; }

        public List<Vector3> Path => CurrentPath?.ToList();

        public BasicVehicle PlayerVehicle { get; private set; }

        public bool Straving { get; private set; }

        public Vector3 TargetPosition { get; private set; }

        public float TargetRotation { get; private set; }

        public int TryCount { get; private set; }

        private DateTime LastAction { get; set; }

        private Random Rnd { get; }

        private DateTime StrafeEnd { get; set; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (DateTime.Now - LastAction < TimeSpan.FromMilliseconds(250))
            {
                return;
            }

            LastAction = DateTime.Now;

            if ((DateTime.Now - LastLastPositionUpdate > TimeSpan.FromMilliseconds(1000) || TryCount > 2))
            {
                Reset();
                return;
            }

            if (CurrentPath.Count == 0)
            {
                TryCount = 0;
                List<Vector3> nodes = new List<Vector3>();

                if (WowInterface.ObjectManager.Player.Position.GetDistance(TargetPosition) > 5)
                {
                    List<Vector3> getPathResult = WowInterface.PathfindingHandler.GetPath((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, TargetPosition);

                    if (getPathResult != null && getPathResult.Count > 0)
                    {
                        nodes.AddRange(getPathResult);
                    }
                }
                else
                {
                    Vector3 moveAlongSurfaceResult = WowInterface.PathfindingHandler.MoveAlongSurface((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, TargetPosition);

                    if (moveAlongSurfaceResult != default && moveAlongSurfaceResult != Vector3.Zero)
                    {
                        nodes.Add(moveAlongSurfaceResult);
                    }
                }

                if (nodes == null || nodes.Count == 0)
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

            if (distanceToTargetPosition > 4096)
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

            Vector3 positionToGoTo = targetPosition;
            bool updateForces = true;

            switch (MovementAction)
            {
                case MovementAction.Moving:
                    forces.Add(PlayerVehicle.Seek(positionToGoTo, 1));
                    forces.Add(PlayerVehicle.AvoidObstacles(2));
                    break;

                case MovementAction.DirectMoving:
                    WowInterface.CharacterManager.MoveToPosition(targetPosition);
                    updateForces = false;
                    break;

                case MovementAction.Following:
                    forces.Add(PlayerVehicle.Seek(positionToGoTo, 1));
                    forces.Add(PlayerVehicle.Seperate(0.5f));
                    forces.Add(PlayerVehicle.AvoidObstacles(2));
                    break;

                case MovementAction.Chasing:
                    forces.Add(PlayerVehicle.Seek(positionToGoTo, 1));
                    break;

                case MovementAction.Fleeing:
                    forces.Add(PlayerVehicle.Flee(positionToGoTo, 1));
                    break;

                case MovementAction.Evading:
                    forces.Add(PlayerVehicle.Evade(positionToGoTo, 1, TargetRotation));
                    break;

                case MovementAction.Wandering:
                    forces.Add(PlayerVehicle.Wander(1));
                    break;

                case MovementAction.Stuck:
                    forces.Add(PlayerVehicle.Unstuck(1));
                    break;

                default:
                    return;
            }

            if (DateTime.Now - LastJumpCheck > TimeSpan.FromMilliseconds(250))
            {
                double distanceTraveled = LastPosition.GetDistance(WowInterface.ObjectManager.Player.Position);
                if ((LastPosition.X == 0 && LastPosition.Y == 0 && LastPosition.Z == 0) || distanceTraveled < 0.01)
                {
                    ++TryCount;
                }
                else
                {
                    TryCount = 0;

                    // if (Straving)
                    // {
                    //     WowInterface.HookManager.LuaDoString("StrafeLeftStop();MoveBackwardStop();StrafeRightStop();MoveBackwardStop();");
                    //     Straving = false;
                    // }
                }

                // if the target position is higher than us, jump
                if (TryCount > 1 || (distanceToTargetPosition < 3 && currentPosition.Z + 2 < targetPosition.Z))
                {
                    WowInterface.CharacterManager.Jump();
                    TryCount = 0;

                    // if (DateTime.Now > StrafeEnd)
                    // {
                    //     int msToStrafe = Rnd.Next(1000, 5000);
                    //
                    //     if (Rnd.Next(0, 2) == 0)
                    //     {
                    //         WowInterface.HookManager.LuaDoString("StrafeLeftStart();MoveBackwardStart();");
                    //         Straving = true;
                    //     }
                    //     else
                    //     {
                    //         WowInterface.HookManager.LuaDoString("StrafeRightStart();MoveBackwardStart();");
                    //         Straving = true;
                    //     }
                    //
                    //     StrafeEnd = DateTime.Now + TimeSpan.FromMilliseconds(msToStrafe + 200);
                    // }
                }

                if (updateForces && !Straving)
                {
                    PlayerVehicle.Update(forces);
                }

                LastPosition = WowInterface.ObjectManager.Player.Position;
                LastLastPositionUpdate = DateTime.Now;
                LastJumpCheck = DateTime.Now;
            }

            LastTargetPosition = WowInterface.ObjectManager.Player.Position;
            HasMoved = true;
        }

        public void Reset()
        {
            WowInterface.HookManager.StopClickToMoveIfActive(WowInterface.ObjectManager.Player);
            MovementAction = MovementAction.None;
            CurrentPath = new Queue<Vector3>();
            HasMoved = false;
            TryCount = 0;
            LastLastPositionUpdate = DateTime.Now;
        }

        public void SetMovementAction(MovementAction state, Vector3 position, float targetRotation = 0f)
        {
            if (MovementAction != state)
            {
                Reset();
                MovementAction = state;
            }

            TargetPosition = position;
            TargetRotation = targetRotation;
        }
    }
}