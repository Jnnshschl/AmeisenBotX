using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AmeisenBotX.Common.Math
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

        /// <summary>
        /// Clamps an angle to 0 - 2*PI
        /// </summary>
        /// <param name="angle">Current angle</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampAngles(float angle)
        {
            const float MAX_ANGLE = MathF.PI * 2.0f;

            if (angle < 0.0f)
            {
                angle += MAX_ANGLE;
            }
            else if (angle > MAX_ANGLE)
            {
                angle -= MAX_ANGLE;
            }

            return angle;
        }

        /// <summary>
        /// Get the amount of rotation needed to face the target position.
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="rotation">Current rotation</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns>Amount of rotation needed to face the target position</returns>
        public static float GetAngleDiff(Vector3 position, float rotation, Vector3 targetPosition)
        {
            return GetFacingAngle(position, targetPosition) - rotation;
        }

        /// <summary>
        /// Get the angle from a position to a target position.
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns>Angle of position to target position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetFacingAngle(Vector3 position, Vector3 targetPosition)
        {
            return ClampAngles(MathF.Atan2(targetPosition.Y - position.Y, targetPosition.X - position.X));
        }

        /// <summary>
        /// Get the center position of a position list.
        /// </summary>
        /// <param name="positions">Positions to get the center of</param>
        /// <returns>Center of the positions</returns>
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

        /// <summary>
        /// Calculate the percentage value.
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="max">Max value</param>
        /// <returns>Percentage</returns>
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