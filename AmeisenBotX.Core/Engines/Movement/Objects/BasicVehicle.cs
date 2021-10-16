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
        public BasicVehicle(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;
        }

        public delegate void MoveCharacter(Vector3 positionToGoTo);

        public bool IsOnWaterSurface { get; set; }

        public Vector3 Velocity { get; private set; }

        private AmeisenBotInterfaces Bot { get; }

        private AmeisenBotConfig Config { get; }

        public Vector3 AvoidObstacles(float multiplier)
        {
            Vector3 acceleration = new(0, 0, 0);

            acceleration += GetObjectForceAroundMe<IWowObject>();
            acceleration += GetNearestBlacklistForce(12);

            acceleration.Limit(Config.MovementSettings.MaxAcceleration);
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
            Vector3 currentPosition = Bot.Player.Position;
            Vector3 desired = currentPosition;
            float distanceToTarget = currentPosition.GetDistance(position);

            desired -= position;
            desired.Normalize2D(desired.GetMagnitude2D());

            float maxVelocity = Config.MovementSettings.MaxVelocity;

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
                float maxSteeringCombat = Config.MovementSettings.MaxSteeringCombat;

                if (Bot.Player.IsMounted)
                {
                    maxSteeringCombat *= 2;
                }

                steering.Limit(maxSteeringCombat);
            }
            else
            {
                float maxSteering = Config.MovementSettings.MaxSteering;

                if (Bot.Player.IsMounted)
                {
                    maxSteering *= 2;
                }

                steering.Limit(maxSteering);
            }

            Vector3 acceleration = new(0, 0, 0);
            acceleration += steering;

            if (Bot.Player.IsInCombat)
            {
                float maxAcceleration = Config.MovementSettings.MaxAccelerationCombat;

                if (Bot.Player.IsMounted)
                {
                    maxAcceleration *= 2;
                }

                acceleration.Limit(maxAcceleration);
            }
            else
            {
                float maxAcceleration = Config.MovementSettings.MaxAcceleration;

                if (Bot.Player.IsMounted)
                {
                    maxAcceleration *= 2;
                }

                acceleration.Limit(maxAcceleration);
            }

            acceleration.Multiply(multiplier);
            return acceleration;
        }

        public Vector3 Pursuit(Vector3 position, float multiplier, float targetRotation, float targetVelocity = 2f)
        {
            Vector3 positionAhead = CalculateFuturePosition(position, targetRotation, targetVelocity);
            return Seek(positionAhead, multiplier);
        }

        public void Reset()
        {
            Velocity = new Vector3();
        }

        public Vector3 Seek(Vector3 position, float multiplier)
        {
            Vector3 currentPosition = Bot.Player.Position;
            Vector3 desired = position;
            float distanceToTarget = currentPosition.GetDistance(position);

            desired -= currentPosition;
            desired.Normalize2D(desired.GetMagnitude2D());

            float maxVelocity = Config.MovementSettings.MaxVelocity;

            if (Bot.Player.IsMounted)
            {
                maxVelocity *= 2;
            }

            desired.Multiply(maxVelocity);

            if (distanceToTarget < 4)
            {
                desired.Multiply(distanceToTarget / 4);
            }

            Vector3 steering = desired;
            steering -= Velocity;

            // float maxSteering = Bot.Player.IsInCombat ? Bot.MovementSettings.MaxSteeringCombat : Bot.MovementSettings.MaxSteering;

            if (Bot.Player.IsMounted)
            {
                maxVelocity *= 2;
            }

            steering.Limit(maxVelocity);

            Vector3 acceleration = new(0, 0, 0);
            acceleration += steering;

            float maxAcceleration = Bot.Player.IsInCombat ? Config.MovementSettings.MaxAccelerationCombat : Config.MovementSettings.MaxAcceleration;

            if (Bot.Player.IsMounted)
            {
                maxAcceleration *= 2;
            }

            acceleration.Limit(maxAcceleration);
            acceleration.Multiply(multiplier);
            return acceleration;
        }

        public Vector3 Seperate(float multiplier)
        {
            Vector3 acceleration = new(0, 0, 0);
            acceleration += GetObjectForceAroundMe<IWowPlayer>(Config.MovementSettings.SeperationDistance);

            float maxAcceleration = Config.MovementSettings.MaxAcceleration;

            if (Bot.Player.IsMounted)
            {
                maxAcceleration *= 2;
            }

            acceleration.Limit(maxAcceleration);
            acceleration.Multiply(multiplier);
            return acceleration;
        }

        public Vector3 Unstuck(float multiplier)
        {
            Vector3 positionBehindMe = CalculatPositionBehind(Bot.Player.Position, Bot.Player.Rotation, 8);
            return Seek(positionBehindMe, multiplier);
        }

        public void Update(MoveCharacter moveCharacter, MovementAction movementAction, Vector3 targetPosition, float rotation = 0f)
        {
            if (movementAction == MovementAction.DirectMove)
            {
                moveCharacter?.Invoke(targetPosition);
                return;
            }

            List<Vector3> forces = GetForces(movementAction, targetPosition, rotation);

            foreach (Vector3 force in forces)
                Velocity += force;

            if (IsOnWaterSurface && Velocity.Z > 0f)
            {
                Velocity = new(Velocity.X, Velocity.Y, 0f);
            }

            float maxVelocity = Config.MovementSettings.MaxVelocity;

            if (Bot.Player.IsMounted)
            {
                maxVelocity *= 2;
            }

            Velocity.Limit(maxVelocity);

            Vector3 currentPosition = Bot.Player.Position;
            currentPosition.Add(Velocity);

            moveCharacter?.Invoke(currentPosition);
        }

        public Vector3 Wander(float multiplier)
        {
            // TODO: implement some sort of radius where the target wanders around.
            //       maybe add a very weak force keeping it inside a given circle...
            // TODO: implement some sort of delay so that the target is not constantly walking
            Random rnd = new();
            Vector3 currentPosition = Bot.Player.Position;

            Vector3 newRandomPosition = new(0, 0, 0);
            newRandomPosition += CalculateFuturePosition(currentPosition, Bot.Player.Rotation, ((float)rnd.NextDouble() * 4.0f) + 4.0f);

            // rotate the vector by random amount of degrees
            newRandomPosition.Rotate(rnd.Next(-14, 14));

            return Seek(newRandomPosition, multiplier);
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

        private static Vector3 CalculatPositionBehind(Vector3 position, float targetRotation, float targetVelocity)
        {
            float rotation = targetRotation + MathF.PI;

            if (rotation > 2.0f * MathF.PI)
            {
                rotation -= 2.0f * MathF.PI;
            }

            float x = position.X + (MathF.Cos(rotation) * targetVelocity);
            float y = position.Y + (MathF.Sin(rotation) * targetVelocity);

            return new()
            {
                X = x,
                Y = y,
                Z = position.Z
            };
        }

        private List<Vector3> GetForces(MovementAction movementAction, Vector3 targetPosition, float rotation = 0f, bool enablePlayerForces = false)
        {
            List<Vector3> forces = new();

            switch (movementAction)
            {
                case MovementAction.Move:
                    forces.Add(Seek(targetPosition, 1f));

                    if (enablePlayerForces)
                    {
                        forces.Add(Seperate(0.05f));
                    }

                    forces.Add(AvoidObstacles(0.5f));
                    break;

                case MovementAction.Follow:
                    forces.Add(Seek(targetPosition, 1f));
                    forces.Add(Seperate(0.03f));
                    forces.Add(AvoidObstacles(0.03f));
                    break;

                case MovementAction.Chase:
                    forces.Add(Seek(targetPosition, 1f));
                    break;

                case MovementAction.Flee:
                    Vector3 fleeForce = Flee(targetPosition, 1f);
                    fleeForce.Z = 0; // set z to zero to avoid going under the terrain
                    forces.Add(fleeForce);
                    break;

                case MovementAction.Evade:
                    forces.Add(Evade(targetPosition, 1f, rotation));
                    break;

                case MovementAction.Wander:
                    Vector3 wanderForce = Wander(1f);
                    wanderForce.Z = 0; // set z to zero to avoid going under the terrain
                    forces.Add(wanderForce);
                    break;

                case MovementAction.Unstuck:
                    forces.Add(Unstuck(1f));
                    break;

                case MovementAction.None:
                    break;

                case MovementAction.DirectMove:
                    break;

                default:
                    break;
            }

            return forces;
        }

        private Vector3 GetNearestBlacklistForce(float maxDistance = 8.0f)
        {
            Vector3 force = new(0, 0, 0);

            if (Bot.Db.TryGetBlacklistPosition((int)Bot.Objects.MapId, Bot.Player.Position, maxDistance, out IEnumerable<Vector3> nodes))
            {
                force += Flee(nodes.First(), 0.5f);
            }

            return force;
        }

        private Vector3 GetObjectForceAroundMe<T>(float maxDistance = 3.0f) where T : IWowObject
        {
            Vector3 force = new(0, 0, 0);
            Vector3 vehiclePosition = Bot.Player.Position;
            int count = 0;

            List<(Vector3, float)> objectDistances = new();

            // we need to know every objects position and distance
            // to later apply a force pushing us back from it that
            // is relational to the objects distance.
            IEnumerable<T> objects = Bot.Objects.WowObjects.OfType<T>();

            for (int i = 0; i < objects.Count(); ++i)
            {
                T obj = objects.ElementAt(i);
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
                force += Flee(objectDistances[i].Item1, objectDistances[i].Item2 * normalizingMultiplier);
                count++;
            }

            // return the average force
            return force / count;
        }
    }
}