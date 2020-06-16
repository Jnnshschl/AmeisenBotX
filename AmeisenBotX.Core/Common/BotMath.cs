using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Common
{
    public class BotMath
    {
        public static Vector3 CalculatePositionAround(Vector3 position, float rotation, double angle, double distance = 2.0)
        {
            double s = Math.Round(Math.Sin(rotation + angle), 4);
            double c = Math.Round(Math.Cos(rotation + angle), 4);

            float x = Convert.ToSingle((position.X + distance) * c);
            float y = Convert.ToSingle((position.Y + distance) * s);

            return new Vector3(x, y, position.Z);
        }

        public static Vector3 CalculatePositionBehind(Vector3 position, float rotation, double distanceToMove = 2.0)
        {
            return CalculatePositionAround(position, rotation, Math.PI, distanceToMove);
        }

        /// <summary>
        /// Caps a Vector3's values to the given max value
        /// </summary>
        /// <param name="vector">The Vector3 to cap</param>
        /// <param name="max">The maximun value a value in the vector is allowed to have</param>
        /// <returns>The capped Vector</returns>
        public static Vector3 CapVector3(Vector3 vector, float max)
            => new Vector3(Math.Min(vector.X, max), Math.Min(vector.Y, max), Math.Min(vector.Z, max));

        public static float GetFacingAngle2D(Vector3 position, Vector3 targetPosition)
            => ClampAngles(Convert.ToSingle(Math.Atan2(targetPosition.Y - position.Y, targetPosition.X - position.X)));

        public static bool IsFacing(Vector3 position, float rotation, Vector3 targetPosition, double minRotation = 0.7, double maxRotation = 1.3)
        {
            float f = GetFacingAngle2D(position, targetPosition);
            return (f >= (rotation * minRotation)) && (f <= (rotation * maxRotation));
        }

        private static float ClampAngles(float rotation)
        {
            if (rotation < 0.0f)
            {
                rotation += (float)Math.PI * 2.0f;
            }
            else if (rotation > (float)Math.PI * 2)
            {
                rotation -= (float)Math.PI * 2.0f;
            }

            return rotation;
        }
    }
}