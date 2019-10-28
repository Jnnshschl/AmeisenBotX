using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Pathfinding;
using AmeisenBotX.Pathfinding.Objects;
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

            Reset();
        }

        public Queue<Vector3> CurrentPath { get; private set; }

        public Vector3 SelectedWaypoint { get; private set; }

        public Vector3 LastPosition { get; private set; }

        public MovementSettings Settings { get; private set; }

        public bool GetNextStep(Vector3 currentPosition, float currentRotation, out Vector3 positionToGoTo, out bool needToJump)
        {
            if (CurrentPath == null || CurrentPath.Count == 0)
            {
                positionToGoTo = Vector3.Zero;
                needToJump = false;
                return false;
            }

            double distance = currentPosition.GetDistance(SelectedWaypoint);

            if(SelectedWaypoint != Vector3.Zero && distance > 1024)
            {
                Reset();
                positionToGoTo = Vector3.Zero;
                needToJump = false;
                return false;
            }

            if (SelectedWaypoint == Vector3.Zero
                || distance < Settings.WaypointDoneThreshold)
            {
                do
                {
                    SelectedWaypoint = CurrentPath.Dequeue();
                    distance = currentPosition.GetDistance(SelectedWaypoint);
                } while (distance < 3.2 && CurrentPath.Count > 0);
            }

            Vector3 force = Vector3.Zero;

            Vector3 waypointForce = SelectedWaypoint - currentPosition;
            force += BotMath.CapVector3(waypointForce, Settings.MaxVelocity);

            //// Vector3 obstacleForce = Vector3.Zero;
            //// force -= BotMath.CapVector3(obstacleForce, Settings.MaxVelocity);

            Vector3 velocity = BotMath.CapVector3(force, Settings.MaxVelocity);
            Vector3 newPosition = currentPosition + velocity;

            double distanceTraveled = currentPosition.GetDistance(LastPosition);
            needToJump = distanceTraveled > 0 && distanceTraveled < 0.05;

            LastPosition = currentPosition;
            positionToGoTo = newPosition;
            return true;
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
            CurrentPath = new Queue<Vector3>();
            LastPosition = Vector3.Zero;
            SelectedWaypoint = Vector3.Zero;
        }

        private bool NeedToJumpOrUnstuck(Vector3 currentPosition, float currentRotation, double distanceTraveled)
        {
            return false;
        }

        private Vector3 CalculatePositionBehindMe(Vector3 currentPosition, float currentRotation)
        {
            double x = currentPosition.X + Math.Cos(currentRotation + Math.PI);
            double y = currentPosition.Y + Math.Sin(currentRotation + Math.PI);

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
