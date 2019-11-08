using AmeisenBotX.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Common
{
    public class BotMath
    {
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

        /// <summary>
        /// Caps a Vector3's values to the given max value
        /// </summary>
        /// <param name="vector">The Vector3 to cap</param>
        /// <param name="max">The maximun value a value in the vector is allowed to have</param>
        /// <returns>The capped Vector</returns>
        public static Vector3 CapVector3(Vector3 vector, float max)
            => new Vector3(
                vector.X < 0 ?
                    vector.X <= max * -1 ? max * -1 : vector.X
                    : vector.X >= max ? max : vector.X,
                vector.Y < 0 ?
                    vector.Y <= max * -1 ? max * -1 : vector.Y
                    : vector.Y >= max ? max : vector.Y,
                vector.Z < 0 ?
                    vector.Z <= max * -1 ? max * -1 : vector.Z
                    : vector.Z >= max ? max : vector.Z);
    }
}