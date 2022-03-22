using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Movement.Objects
{
    public class BasicVehicle
    {
        public BasicVehicle(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        public delegate void MoveCharacter(Vector3 positionToGoTo);

        public DateTime LastUpdate { get; private set; }

        public Vector3 Velocity { get; private set; }

        private AmeisenBotInterfaces Bot { get; }

        public Vector3 AvoidObstacles(float maxSteering, float maxVelocity, float multiplier)
        {
            Vector3 acceleration = GetObjectForceAroundMe<IWowGameobject>(maxSteering, maxVelocity)
                                 + GetNearestBlacklistForce(maxSteering, maxVelocity, 12.0f);

            return acceleration.Truncated(maxSteering) * multiplier;
        }

        public Vector3 Evade(Vector3 position, float maxSteering, float maxVelocity, float multiplier, float targetRotation, float targetVelocity = 2.0f)
        {
            Vector3 positionAhead = CalculateFuturePosition(position, targetRotation, targetVelocity);
            return Flee(positionAhead, maxSteering, maxVelocity, multiplier);
        }

        public Vector3 Flee(Vector3 position, float maxSteering, float maxVelocity, float multiplier)
        {
            Vector3 currentPosition = Bot.Player.Position;
            Vector3 desired = currentPosition;
            float distanceToTarget = currentPosition.GetDistance(position);

            desired -= position;
            desired.Normalize2D(desired.GetMagnitude2D());

            if (Bot.Player.IsMounted)
            {
                maxVelocity *= 2;
            }

            desired.Multiply(maxVelocity);

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

            if (Bot.Player.IsInCombat)
            {
                float maxSteeringCombat = maxSteering;

                if (Bot.Player.IsMounted)
                {
                    maxSteeringCombat *= 2;
                }

                steering.Truncate(maxSteeringCombat);
            }
            else
            {
                if (Bot.Player.IsMounted)
                {
                    maxSteering *= 2;
                }

                steering.Truncate(maxSteering);
            }

            Vector3 acceleration = new();
            acceleration += steering;

            if (Bot.Player.IsInCombat)
            {
                if (Bot.Player.IsMounted)
                {
                    maxVelocity *= 2;
                }

                acceleration.Truncate(maxVelocity);
            }
            else
            {
                if (Bot.Player.IsMounted)
                {
                    maxVelocity *= 2;
                }

                acceleration.Truncate(maxVelocity);
            }

            acceleration.Multiply(multiplier);
            return acceleration;
        }

        public Vector3 Pursuit(Vector3 position, float maxSteering, float maxVelocity, float multiplier, float targetRotation, float targetVelocity = 2.0f)
        {
            Vector3 positionAhead = CalculateFuturePosition(position, targetRotation, targetVelocity);
            return Seek(positionAhead, maxSteering, maxVelocity, multiplier);
        }

        public Vector3 Seek(Vector3 position, float maxSteering, float maxVelocity, float multiplier)
        {
            Vector3 desiredVelocity = (position - Bot.Player.Position).Normalized() * maxVelocity;

            const float slowRad = 3.5f;
            float distance = Bot.Player.DistanceTo(position);

            if (distance < slowRad)
            {
                desiredVelocity *= distance / slowRad;
            }

            return (desiredVelocity - Velocity).Truncated(maxSteering) * multiplier;
        }

        public Vector3 Seperate(float seperationDistance, float maxVelocity, float multiplier)
        {
            return GetObjectForceAroundMe<IWowPlayer>(seperationDistance, maxVelocity) * multiplier;
        }

        public Vector3 Unstuck(float maxSteering, float maxVelocity, float multiplier)
        {
            Vector3 positionBehindMe = BotMath.CalculatePositionBehind(Bot.Player.Position, Bot.Player.Rotation, 8.0f);
            return Seek(positionBehindMe, maxSteering, maxVelocity, multiplier);
        }

        public void Update(MoveCharacter moveCharacter, MovementAction movementAction, Vector3 targetPosition, float rotation, float maxSteering, float maxVelocity, float seperationDistance)
        {
            if (movementAction == MovementAction.DirectMove)
            {
                moveCharacter?.Invoke(targetPosition);
                return;
            }

            // adjust max steering based on time passed since last Update() call
            float timedelta = (float)(DateTime.UtcNow - LastUpdate).TotalSeconds;
            float maxSteeringNormalized = maxSteering * timedelta;

            Vector3 totalforce = GetForce(movementAction, targetPosition, rotation, maxSteeringNormalized, maxVelocity, seperationDistance);
            Velocity += totalforce;
            Velocity.Truncate(maxVelocity);

            moveCharacter?.Invoke(Bot.Player.Position + Velocity);
            LastUpdate = DateTime.UtcNow;
        }

        public Vector3 Wander(float multiplier, float maxSteering, float maxVelocity)
        {
            // TODO: implement some sort of radius where the target wanders around. maybe add a very
            // weak force keeping it inside a given circle...
            // TODO: implement some sort of delay so that the target is not constantly walking
            Random rnd = new();
            Vector3 currentPosition = Bot.Player.Position;

            Vector3 newRandomPosition = new();
            newRandomPosition += CalculateFuturePosition(currentPosition, Bot.Player.Rotation, ((float)rnd.NextDouble() * 4.0f) + 4.0f);

            // rotate the vector by random amount of degrees
            newRandomPosition.Rotate(rnd.Next(-14, 14));

            return Seek(newRandomPosition, maxSteering, maxVelocity, multiplier);
        }

        private static Vector3 CalculateFuturePosition(Vector3 position, float targetRotation, float targetVelocity)
        {
            float rotation = targetRotation;
            float x = position.X + (MathF.Cos(rotation) * targetVelocity);
            float y = position.Y + (MathF.Sin(rotation) * targetVelocity);

            return new()
            {
                X = x,
                Y = y,
                Z = position.Z
            };
        }

        private Vector3 GetForce(MovementAction movementAction, Vector3 targetPosition, float rotation, float maxSteering, float maxVelocity, float seperationDistance)
        {
            return movementAction switch
            {
                MovementAction.Move => Seek(targetPosition, maxSteering, maxVelocity, 0.9f)
                                     + AvoidObstacles(maxSteering, maxVelocity, 0.05f)
                                     + Seperate(seperationDistance, maxVelocity, 0.05f),

                MovementAction.Follow => Seek(targetPosition, maxSteering, maxVelocity, 0.9f)
                                       + AvoidObstacles(maxSteering, maxVelocity, 0.05f)
                                       + Seperate(seperationDistance, maxVelocity, 0.05f),

                MovementAction.Chase => Seek(targetPosition, maxSteering, maxVelocity, 1.0f),
                MovementAction.Flee => Flee(targetPosition, maxSteering, maxVelocity, 1.0f).ZeroZ(),
                MovementAction.Evade => Evade(targetPosition, maxSteering, maxVelocity, 1.0f, rotation),
                MovementAction.Wander => Wander(maxSteering, maxVelocity, 1.0f).ZeroZ(),
                MovementAction.Unstuck => Unstuck(maxSteering, maxVelocity, 1.0f),

                _ => Vector3.Zero,
            };
        }

        private Vector3 GetNearestBlacklistForce(float maxSteering, float maxVelocity, float maxDistance = 8.0f)
        {
            Vector3 force = new();

            if (Bot.Db.TryGetBlacklistPosition((int)Bot.Objects.MapId, Bot.Player.Position, maxDistance, out IEnumerable<Vector3> nodes))
            {
                force += Flee(nodes.First(), 0.5f, maxSteering, maxVelocity);
            }

            return force;
        }

        private Vector3 GetObjectForceAroundMe<T>(float maxSteering, float maxVelocity, float maxDistance = 3.0f) where T : IWowObject
        {
            int count = 0;
            Vector3 force = new();
            Vector3 vehiclePosition = Bot.Player.Position;
            List<(Vector3, float)> objectDistances = new();

            // we need to know every objects position and distance to later apply a force pushing us
            // back from it that is relational to the objects distance.

            foreach (T obj in Bot.Objects.All.OfType<T>())
            {
                float distance = obj.Position.GetDistance(vehiclePosition);

                if (distance < maxDistance)
                {
                    objectDistances.Add((obj.Position, distance));
                }
            }

            if (objectDistances.Count == 0)
            {
                return Vector3.Zero;
            }

            // get the biggest distance to normalize the fleeing forces
            float normalizingMultiplier = objectDistances.Max(e => e.Item2);

            for (int i = 0; i < objectDistances.Count; ++i)
            {
                force += Flee(objectDistances[i].Item1, objectDistances[i].Item2 * normalizingMultiplier, maxSteering, maxVelocity);
                count++;
            }

            // return the average force
            return force / count;
        }
    }
}