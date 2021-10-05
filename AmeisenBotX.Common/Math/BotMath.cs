using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AmeisenBotX.Common.Math
{
    public static class BotMath
    {
        public const float M_SQRT1_2 = 0.707106781186547524401f;
        public const float M_SQRT2 = 1.41421356237309504880f;
        public const float MAX_ANGLE = MathF.PI * 2.0f;

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
            switch (angle)
            {
                case < 0.0f:
                    angle += MAX_ANGLE;
                    break;

                case > MAX_ANGLE:
                    angle -= MAX_ANGLE;
                    break;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetSlope(Vector3 startPoint, Vector3 endPoint, bool toPercentage = false)
        {
            //Calculate the values of the run and rise
            float run = System.Math.Abs(endPoint.X - startPoint.X);
            float rise = System.Math.Abs(endPoint.Y - startPoint.Y);

            if (!toPercentage)
                return rise / run;

            return (rise / run) * 100;
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

            return value / (float)max * 100.0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SlopeGradientAngle(Vector3 startPoint, Vector3 endPoint)
        {
            float slope = GetSlope(startPoint, endPoint, true);
            //Calculates the arctan to get the radians (arctan(alpha) = rise / run)
            double radAngle = System.Math.Atan(slope / 100);
            //Converts the radians in degrees
            double degAngle = radAngle * 180 / System.Math.PI;

            return degAngle;
        }
    }
}