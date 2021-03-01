using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AmeisenBotX.Core.Common
{
    public static class BotMath
    {
        public static Vector3 CalculatePositionAround(Vector3 position, float rotation, float angle, float distance = 2.0f)
        {
            float x = position.X + MathF.Cos(rotation + angle) * distance;
            float y = position.Y + MathF.Sin(rotation + angle) * distance;
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

        public static float GetAngleDiff(Vector3 position, float rotation, Vector3 targetPosition)
        {
            return GetFacingAngle(position, targetPosition) - rotation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetFacingAngle(Vector3 position, Vector3 targetPosition)
        {
            return ClampAngles(MathF.Atan2(targetPosition.Y - position.Y, targetPosition.X - position.X));
        }

        public static Vector3 GetMeanPosition(IEnumerable<Vector3> positions)
        {
            Vector3 meanPosition = new();
            float count = 0;

            foreach (Vector3 position in positions)
            {
                meanPosition += position;
                ++count;
            }

            return meanPosition / count;
        }

        public static bool IsFacing(Vector3 position, float rotation, Vector3 targetPosition, float maxAngleDiff = 1.5f)
        {
            return MathF.Abs(GetAngleDiff(position, rotation, targetPosition)) < maxAngleDiff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(float a, float min, float max)
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
        public static float Percentage(int value, int max)
        {
            if (value == 0 || max == 0)
            {
                return 0;
            }
            else
            {
                return value / (float)max * 100.0f;
            }
        }
    }
}