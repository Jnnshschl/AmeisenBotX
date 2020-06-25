using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Movement.Objects
{
    public class BasicVehicle
    {
        public BasicVehicle(WowInterface wowInterface, float maxSteering, float maxVelocity, float maxAcceleration)
        {
            WowInterface = wowInterface;
            Velocity = new Vector3(0, 0, 0);
            MaxSteering = maxSteering;
            MaxVelocity = maxVelocity;
            MaxAcceleration = maxAcceleration;
        }

        public delegate void MoveCharacter(Vector3 positionToGoTo);

        public event MoveCharacter OnMoveCharacter;

        public float MaxAcceleration { get; private set; }

        public float MaxSteering { get; private set; }

        public float MaxVelocity { get; private set; }

        public Vector3 Velocity { get; private set; }

        private WowInterface WowInterface { get; }

        public Vector3 AvoidObstacles(float multiplier)
        {
            Vector3 acceleration = new Vector3(0, 0, 0);

            acceleration += GetObjectForceAroundMe<WowObject>(12);
            // acceleration += GetNearestBlacklistForce(12);

            acceleration.Limit(MaxAcceleration);
            acceleration.Multiply(multiplier);

            return acceleration;
        }

        public Vector3 Evade(Vector3 position, float multiplier, float targetRotation, float targetVelocity = 2f)
        {
            Vector3 positionAhead = CalculateFuturePosition(position, targetRotation, targetVelocity);
            return Flee(positionAhead, multiplier);
        }

        public Vector3 Flee(Vector3 position, float multiplier)
        {
            Vector3 currentPosition = WowInterface.ObjectManager.Player.Position;
            Vector3 desired = currentPosition;
            float distanceToTarget = Convert.ToSingle(currentPosition.GetDistance(position));

            desired -= position;
            desired.Normalize(desired.GetMagnitude());
            desired.Multiply(MaxVelocity);

            if (distanceToTarget > 20)
            {
                float slowdownMultiplier = 20 / distanceToTarget;

                if (slowdownMultiplier < 0.1)
                {
                    slowdownMultiplier = 0;
                }

                desired.Multiply(slowdownMultiplier);
            }

            Vector3 steering = desired;
            steering -= Velocity;
            steering.Limit(MaxSteering);

            Vector3 acceleration = new Vector3(0, 0, 0);
            acceleration += steering;
            acceleration.Limit(MaxAcceleration);
            acceleration.Multiply(multiplier);
            return acceleration;
        }

        public Vector3 Pursuit(Vector3 position, float multiplier, float targetRotation, float targetVelocity = 2f)
        {
            Vector3 positionAhead = CalculateFuturePosition(position, targetRotation, targetVelocity);
            return Seek(positionAhead, multiplier);
        }

        public Vector3 Seek(Vector3 position, float multiplier)
        {
            Vector3 currentPosition = WowInterface.ObjectManager.Player.Position;
            Vector3 desired = position;
            float distanceToTarget = Convert.ToSingle(currentPosition.GetDistance(position));

            desired -= currentPosition;
            desired.Normalize(desired.GetMagnitude());
            desired.Multiply(MaxVelocity);

            if (distanceToTarget < 4)
            {
                desired.Multiply(distanceToTarget / 4);
            }

            Vector3 steering = desired;
            steering -= Velocity;
            steering.Limit(MaxSteering);

            Vector3 acceleration = new Vector3(0, 0, 0);
            acceleration += steering;
            acceleration.Limit(MaxAcceleration);
            acceleration.Multiply(multiplier);
            return acceleration;
        }

        public Vector3 Seperate(float multiplier)
        {
            Vector3 acceleration = new Vector3(0, 0, 0);
            acceleration += GetObjectForceAroundMe<WowPlayer>(2);
            acceleration.Limit(MaxAcceleration);
            acceleration.Multiply(multiplier);
            return acceleration;
        }

        public Vector3 Unstuck(float multiplier)
        {
            Vector3 positionBehindMe = CalculatPositionBehind(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, 8);
            return Seek(positionBehindMe, multiplier);
        }

        public void Update(List<Vector3> forces)
        {
            for (int i = 0; i < forces.Count; ++i)
            {
                Velocity += forces[i];
            }

            Velocity.Limit(MaxVelocity);

            Vector3 currentPosition = WowInterface.ObjectManager.Player.Position;
            currentPosition.Add(Velocity);

            OnMoveCharacter?.Invoke(currentPosition);
        }

        public Vector3 Wander(float multiplier)
        {
            // TODO: implement some sort of radius where the target wanders around.
            //       maybe add a very weak force keeping it inside a given circle...
            // TODO: implement some sort of delay so that the target is not constantly walking
            Random rnd = new Random();
            Vector3 currentPosition = WowInterface.ObjectManager.Player.Position;

            Vector3 newRandomPosition = new Vector3(0, 0, 0);
            newRandomPosition += CalculateFuturePosition(currentPosition, WowInterface.ObjectManager.Player.Rotation, Convert.ToSingle((rnd.NextDouble() * 4) + 4));

            // rotate the vector by random amount of degrees
            newRandomPosition.Rotate(rnd.Next(-14, 14));

            return Seek(newRandomPosition, multiplier);
        }

        private static Vector3 CalculateFuturePosition(Vector3 position, float targetRotation, float targetVelocity)
        {
            float rotation = targetRotation;
            double x = position.X + (Math.Cos(rotation) * targetVelocity);
            double y = position.Y + (Math.Sin(rotation) * targetVelocity);

            return new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = position.Z
            };
        }

        private static Vector3 CalculatPositionBehind(Vector3 position, float targetRotation, float targetVelocity)
        {
            float rotation = targetRotation + Convert.ToSingle(Math.PI);

            if (rotation > 2 * Math.PI)
            {
                rotation -= Convert.ToSingle(2 * Math.PI);
            }

            double x = position.X + (Math.Cos(rotation) * targetVelocity);
            double y = position.Y + (Math.Sin(rotation) * targetVelocity);

            return new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = position.Z
            };
        }

        private Vector3 GetNearestBlacklistForce(double maxDistance = 8)
        {
            Vector3 force = new Vector3(0, 0, 0);

            if (WowInterface.BotCache.TryGetBlacklistPosition((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, maxDistance, out List<Vector3> nodes))
            {
                if (nodes.Count > 0)
                {
                    force += Flee(nodes.First(), 0.5f);
                }
            }

            return force;
        }

        private Vector3 GetObjectForceAroundMe<T>(double maxDistance = 16) where T : WowObject
        {
            Vector3 force = new Vector3(0, 0, 0);
            Vector3 vehiclePosition = WowInterface.ObjectManager.Player.Position;
            int count = 0;

            List<(Vector3, double)> objectDistances = new List<(Vector3, double)>();

            // we need to know every objects position and distance
            // to later apply a force pushing us back from it that
            // is relational to the objects distance.
            T[] objects = WowInterface.ObjectManager.WowObjects.OfType<T>().ToArray();

            for (int i = 0; i < objects.Length; ++i)
            {
                double distance = objects[i].Position.GetDistance(vehiclePosition);

                if (distance < maxDistance)
                {
                    objectDistances.Add((objects[i].Position, distance));
                }
            }

            // get the biggest distance to normalize the fleeing forces
            float normalizingMultiplier = Convert.ToSingle(objectDistances.Max(e => e.Item2));

            for (int i = 0; i < objectDistances.Count; ++i)
            {
                force += Flee(objectDistances[i].Item1, Convert.ToSingle(objectDistances[i].Item2) * normalizingMultiplier);
                count++;
            }

            // return the average force
            return force / count;
        }
    }
}