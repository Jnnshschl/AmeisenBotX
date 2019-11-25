using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Movement
{
    public class DefaultMovementEngine
    {
        public DefaultMovementEngine(ObjectManager objectManager, MovementSettings settings = null)
        {
            ObjectManager = objectManager;

            if (settings == null)
            {
                settings = new MovementSettings();
            }

            Settings = settings;

            Reset();
        }

        public Queue<Vector3> CurrentPath { get; private set; }

        public Vector3 LastPosition { get; private set; }

        public Vector3 Acceleration { get; private set; }

        public Vector3 Velocity { get; private set; }

        public MovementSettings Settings { get; private set; }

        private ObjectManager ObjectManager { get; }

        public bool GetNextStep(Vector3 currentPosition, float currentRotation, out Vector3 positionToGoTo, out bool needToJump, bool enableSeperation = false)
        {
            positionToGoTo = new Vector3(0, 0, 0);
            needToJump = false;

            double distance = currentPosition.GetDistance2D(CurrentPath.Peek());
            if ((CurrentPath == null || CurrentPath.Count == 0)
                || (CurrentPath.Peek() != new Vector3(0, 0, 0) && distance > 1024))
            {
                return false;
            }

            if (distance < Settings.WaypointCheckThreshold
                && CurrentPath.Count > 1)
            {
                CurrentPath.Dequeue();
            }

            Vector3 seekForce = Seek(currentPosition, distance);
            seekForce.Multiply(1);

            Vector3 seperationForce = new Vector3(0, 0, 0);
            if (enableSeperation)
            {
                // seperation is more important than seeking
                seperationForce = Seperate(ObjectManager.Player.Position);
                seperationForce.Multiply(4);
            }

            positionToGoTo = currentPosition;
            positionToGoTo.Add(seekForce);
            positionToGoTo.Add(seperationForce);

            Acceleration = new Vector3(0, 0, 0);

            double heightDiff = positionToGoTo.Z - positionToGoTo.Z;
            if (heightDiff < 0)
            {
                heightDiff *= -1;
            }

            double distanceTraveled = currentPosition.GetDistance2D(LastPosition);
            needToJump =  LastPosition != new Vector3(0, 0, 0) && (heightDiff > 1 || distanceTraveled > 0 && distanceTraveled < 0.1);
            LastPosition = currentPosition;
            return true;
        }

        private Vector3 Seek(Vector3 currentPosition, double distance)
        {
            Vector3 desired = CurrentPath.Peek() - currentPosition;
            desired.Normalize(desired.GetMagnitude());

            float multiplier = Convert.ToSingle(distance);
            if (multiplier < 0.2)
            {
                multiplier = 0;
            }
            else if (multiplier > Settings.MaxAcceleration)
            {
                multiplier = Settings.MaxAcceleration;
            }

            Vector3 steering = (desired * multiplier) - Velocity;
            steering.Limit(Settings.MaxSteering);

            Vector3 newVelocity = Velocity + steering;
            Velocity = newVelocity;
            Velocity.Limit(Settings.MaxVelocity);
            return newVelocity;
        }

        private Vector3 Seperate(Vector3 currentPosition)
        {
            Vector3 force = new Vector3(0, 0, 0);

            int playersInRange = 0;
            foreach (WowPlayer player in ObjectManager.WowObjects.OfType<WowPlayer>())
            {
                if (player.Guid == ObjectManager.PlayerGuid)
                {
                    continue;
                }

                double distance = currentPosition.GetDistance2D(player.Position);
                if (distance == 0)
                {
                    // randomly move away
                    Random rnd = new Random();
                    force.X = Convert.ToSingle((rnd.NextDouble() - 3) * 6);
                    force.Y = Convert.ToSingle((rnd.NextDouble() - 3) * 6);
                    force.Z = Convert.ToSingle((rnd.NextDouble() - 3) * 6);
                }
                else
                    if (distance < Settings.SeperationDistance)
                {
                    Vector3 difference = currentPosition;
                    difference.Subtract(player.Position);
                    difference.Normalize(difference.GetMagnitude());

                    Vector3 weightedDifference = difference;
                    weightedDifference.Divide(Convert.ToSingle(distance));

                    force.Add(weightedDifference);
                    playersInRange++;
                }
            }

            if (playersInRange > 0)
            {
                force.Divide(playersInRange);
                force.Normalize(force.GetMagnitude());
                force.Multiply(Settings.MaxVelocity);
                force.Subtract(Velocity);
                force.Limit(Settings.MaxSteering);
            }

            return force;
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
            if (CurrentPath == null)
            {
                CurrentPath = new Queue<Vector3>();
            }

            Acceleration = new Vector3(0, 0, 0);
            CurrentPath.Clear();
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

        private bool NeedToJumpOrUnstuck(Vector3 currentPosition, float currentRotation, double distanceTraveled)
        {
            return false;
        }
    }
}
