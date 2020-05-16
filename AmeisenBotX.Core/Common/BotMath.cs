using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Common
{
    public class BotMath
    {
        public static Vector3 CalculatePositionBehind(Vector3 position, float rotation, double distanceToMove = 2.0)
        {
            double x = position.X + (distanceToMove * Math.Cos(rotation + Math.PI));
            double y = position.Y + (distanceToMove * Math.Sin(rotation + Math.PI));

            return new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = position.Z
            };
        }

        /// <summary>
        /// Caps a Vector3's values to the given max value
        /// </summary>
        /// <param name="vector">The Vector3 to cap</param>
        /// <param name="max">The maximun value a value in the vector is allowed to have</param>
        /// <returns>The capped Vector</returns>
        public static Vector3 CapVector3(Vector3 vector, float max)
            => new Vector3(Math.Min(vector.X, max), Math.Min(vector.Y, max), Math.Min(vector.Z, max));

        public static float GetFacingAngle(Vector3 position, Vector3 targetPosition)
        {
            float angle = (float)Math.Atan2(targetPosition.Y - position.Y, targetPosition.X - position.X);

            if (angle < 0.0f)
            {
                angle += (float)Math.PI * 2.0f;
            }
            else if (angle > (float)Math.PI * 2)
            {
                angle -= (float)Math.PI * 2.0f;
            }

            return angle;
        }

        public static bool IsFacing(Vector3 position, float rotation, Vector3 targetPosition, double minRotation = 0.7, double maxRotation = 1.3)
        {
            float f = GetFacingAngle(position, targetPosition);
            return (f >= (rotation * minRotation)) && (f <= (rotation * maxRotation));
        }
    }
}