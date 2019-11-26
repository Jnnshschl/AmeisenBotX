using AmeisenBotX.Core.Data;
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
        public delegate float GetRotationFunction();
        public delegate Vector3 GetPositionFunction();
        public delegate void MoveToPositionFunction(Vector3 position);

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

        public Vector3 Velocity { get; private set; }

        public float MaxSteering { get; private set; }

        public float MaxVelocity { get; private set; }

        public float MaxAcceleration { get; private set; }

        public ObjectManager ObjectManager { get; }

        private GetRotationFunction GetRotation { get; set; }

        private GetPositionFunction GetPosition { get; set; }

        private MoveToPositionFunction MoveToPosition { get; set; }

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
    }
}
