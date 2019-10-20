using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public class DefaultMovementEngine : IMovementEngine
    {
        public DefaultMovementEngine(MovementSettings settings = null)
        {
            if (settings == null)
            {
                settings = new MovementSettings();
            }

            Settings = settings;
        }

        public Queue<Vector3> CurrentPath { get; private set; }

        public Vector3 LastWaypoint { get; private set; }

        public int TryCount { get; private set; }

        public int UnstuckTryCount { get; private set; } = 1;

        public MovementSettings Settings { get; private set; }

        public MovementEngineState EngineState { get; private set; }

        public bool GetNextStep(Vector3 currentPosition, float currentRotation, out Vector3 positionToGoTo, out bool needToJump)
        {
            double distanceTraveled = currentPosition.GetDistance(LastWaypoint);
            positionToGoTo = LastWaypoint;
            needToJump = false;

            if (!(currentPosition.X == 0 && currentPosition.Y == 0 && currentPosition.Z == 0))
            {
                LastWaypoint = currentPosition;
            }

            if (TryCount < Settings.MaxTries)
            {
                if (CurrentPath.Count > 0)
                {
                    Vector3 currentWaypoint = CurrentPath.Peek();
                    double distance = currentPosition.GetDistance(currentWaypoint);

                    if (distance <= Settings.WaypointCheckThreshold)
                    {
                        CurrentPath.Dequeue();
                    }

                    if (distanceTraveled > 0 && EngineState == MovementEngineState.NORMAL)
                    {
                        TryCount = 0;
                    }

                    needToJump = NeedToJumpOrUnstuck(currentPosition, currentRotation, distanceTraveled);

                    if (CurrentPath.Count > 0)
                    {
                        positionToGoTo = CurrentPath.Peek();
                    }

                    return true;
                }
            }

            Reset();
            return false;
        }

        public void LoadPath(List<Vector3> path)
        {
            CurrentPath = new Queue<Vector3>();
            foreach (Vector3 v in path)
            {
                CurrentPath.Enqueue(v);
            }
        }

        public void PostProcessPath()
        {
            // wont do anything here
        }

        public void Reset()
        {
            EngineState = MovementEngineState.NORMAL;
            CurrentPath = new Queue<Vector3>();
            LastWaypoint = new Vector3(0, 0, 0);
            UnstuckTryCount = 1;
        }

        private bool NeedToJumpOrUnstuck(Vector3 currentPosition, float currentRotation, double distanceTraveled)
        {
            if (distanceTraveled > 0)
            {
                if (EngineState == MovementEngineState.UNSTUCKING)
                {
                    Reset();
                }

                if (distanceTraveled < 0.2)
                {
                    // we ran against something
                    return true;
                }
            }
            else
            {
                TryCount++;

                // we are stuck
                if (EngineState == MovementEngineState.NORMAL && TryCount >= Settings.MaxTries)
                {
                    CurrentPath = new Queue<Vector3>();
                    CurrentPath.Enqueue(CalculatePositionBehindMe(currentPosition, currentRotation));
                    EngineState = MovementEngineState.UNSTUCKING;
                    TryCount = 0;
                    UnstuckTryCount++;
                }
            }

            return false;
        }

        private Vector3 CalculatePositionBehindMe(Vector3 currentPosition, float currentRotation)
        {
            double x = currentPosition.X + (Math.Cos(currentRotation + Math.PI) * (6 * UnstuckTryCount));
            double y = currentPosition.Y + (Math.Sin(currentRotation + Math.PI) * (6 * UnstuckTryCount));

            Vector3 destination = new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = currentPosition.Z
            };

            return destination;
        }
    }
}
