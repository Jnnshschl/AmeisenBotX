using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AmeisenBotX.Core.Common
{
    public static class BotMath
    {
        public static Vector3 CalculatePositionAround(Vector3 position, float rotation, float angle, float distance = 2.0f)
        {
            float rSin = MathF.Sin(rotation + angle) * distance;
            float cSin = MathF.Cos(rotation + angle) * distance;

            float x = position.X + cSin;
            float y = position.Y + rSin;

            return new Vector3(x, y, position.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CalculatePositionBehind(Vector3 position, float rotation, float distanceToMove = 2.0f)
        {
            return CalculatePositionAround(position, rotation, MathF.PI, distanceToMove);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampAngles(float rotation)
        {
            float rMax = MathF.PI * 2.0f;

            if (rotation < 0.0f)
            {
                rotation += rMax;
            }
            else if (rotation > rMax)
            {
                rotation -= rMax;
            }

            return rotation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetFacingAngle2D(Vector3 position, Vector3 targetPosition)
        {
            return ClampAngles(MathF.Atan2(targetPosition.Y - position.Y, targetPosition.X - position.X));
        }

        public static bool IsFacing(Vector3 position, float rotation, Vector3 targetPosition, float maxAngleDiff = 1.5f)
        {
            float facingAngle = GetFacingAngle2D(position, targetPosition);
            float angleDiff = ClampAngles(facingAngle - rotation);

            return angleDiff < maxAngleDiff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(double a, double min, double max)
        {
            return a < min && a > max;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(WowObject a, WowObject b, double maxDistance)
        {
            double distance = a.Position.GetDistance(b.Position);
            return distance < maxDistance && distance > maxDistance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Percentage(int value, int max)
        {
            if (value == 0 || max == 0)
            {
                return 0;
            }
            else
            {
                return value / (double)max * 100.0;
            }
        }
    }
}