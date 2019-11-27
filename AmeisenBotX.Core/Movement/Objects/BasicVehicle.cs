using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Movement.Objects
{
    public class BasicVehicle
    {
        public BasicVehicle(GetPositionFunction getPositionFunction, GetRotationFunction getRotationFunction, MoveToPositionFunction moveToPositionFunction, ObjectManager objectManager, float maxSteering, float maxVelocity, float maxAcceleration)
        {
            Velocity = new Vector3(0, 0, 0);
            MaxSteering = maxSteering;
            MaxVelocity = maxVelocity;
            MaxAcceleration = maxAcceleration;
            GetRotation = getRotationFunction;
            GetPosition = getPositionFunction;
            MoveToPosition = moveToPositionFunction;
            ObjectManager = objectManager;
        }

        public delegate Vector3 GetPositionFunction();

        public delegate float GetRotationFunction();

        public delegate void MoveToPositionFunction(Vector3 position);

        public float MaxAcceleration { get; private set; }

        public float MaxSteering { get; private set; }

        public float MaxVelocity { get; private set; }

        public ObjectManager ObjectManager { get; }

        public Vector3 Velocity { get; private set; }

        private GetPositionFunction GetPosition { get; set; }

        private GetRotationFunction GetRotation { get; set; }

        private MoveToPositionFunction MoveToPosition { get; set; }

        public Vector3 AvoidObstacles(float multiplier)
        {
            Vector3 currentPosition = GetPosition();
            Vector3 acceleration = new Vector3(0, 0, 0);
            acceleration += currentPosition;
            acceleration += GetObjectForceAroundMe<WowObject>();
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
            Vector3 currentPosition = GetPosition();
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
            Vector3 currentPosition = GetPosition();
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

        public Vector3 Unstuck(int multiplier)
        {
            Vector3 positionBehindMe = CalculatPositionnBehind(GetPosition.Invoke(), GetRotation(), 4);
            return Seek(positionBehindMe, multiplier);
        }

        public Vector3 Wander(int multiplier)
        {
            // TODO: implement some sort of radius where the target wanders around.
            //       maybe add a very weak force keeping it inside a given circle...
            // TODO: implement some sort of delay so that the target is not constantly walking
            Random rnd = new Random();
            Vector3 currentPosition = GetPosition();

            Vector3 newRandomPosition = new Vector3(0, 0, 0);
            newRandomPosition += CalculateFuturePosition(currentPosition, GetRotation.Invoke(), Convert.ToSingle((rnd.NextDouble() * 4) + 4));

            // rotate the vector by  random amount of degrees
            newRandomPosition.Rotate(rnd.Next(-14,14));

            return Seek(newRandomPosition, multiplier);
        }

        public Vector3 Seperate(float multiplier)
        {
            Vector3 currentPosition = GetPosition();
            Vector3 acceleration = new Vector3(0, 0, 0);
            acceleration += currentPosition;
            acceleration += GetObjectForceAroundMe<WowPlayer>();
            acceleration.Limit(MaxAcceleration);
            acceleration.Multiply(multiplier);
            return acceleration;
        }

        public void Update(List<Vector3> forces)
        {
            foreach (Vector3 force in forces)
            {
                Velocity += (force);
            }

            Velocity.Limit(MaxVelocity);

            Vector3 currentPosition = GetPosition();
            currentPosition.Add(Velocity);

            MoveToPosition?.Invoke(currentPosition);
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

        private static Vector3 CalculatPositionnBehind(Vector3 position, float targetRotation, float targetVelocity)
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

        private Vector3 GetObjectForceAroundMe<T>(double maxDistance = 16) where T : WowObject
        {
            Vector3 force = new Vector3(0, 0, 0);
            Vector3 vehiclePosition = GetPosition.Invoke();
            int count = 0;

            List<(Vector3, double)> objectDistances = new List<(Vector3, double)>();

            // we need to know every objects position and distance
            // to later apply a force pushing us back from it that
            // is relational to the objects distance.
            foreach (WowObject obj in ObjectManager.WowObjects.OfType<T>())
            {
                double distance = obj.Position.GetDistance(vehiclePosition);

                if (distance < maxDistance)
                {
                    objectDistances.Add((obj.Position, distance));
                }
            }

            // get the biggest distance to normalize the fleeing forces
            float normalizingMultiplier = Convert.ToSingle(objectDistances.Max(e => e.Item2));

            foreach ((Vector3, double) obj in objectDistances)
            {
                force += Flee(obj.Item1, Convert.ToSingle(obj.Item2) * normalizingMultiplier);
                count++;
            }

            // return the average force
            return force / count;
        }
    }
}
