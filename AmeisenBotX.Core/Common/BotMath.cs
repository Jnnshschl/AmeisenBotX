using AmeisenBotX.Pathfinding;
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
    }
}