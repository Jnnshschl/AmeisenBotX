using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Common
{
    public class BotMath
    {
        public static Vector3 CalculatePositionAround(Vector3 position, float rotation, float angle, float distance = 2.0f)
        {
            float rSin = (float)Math.Sin(rotation + angle) * distance;
            float cSin = (float)Math.Cos(rotation + angle) * distance;

            float x = position.X + cSin;
            float y = position.Y + rSin;

            return new Vector3(x, y, position.Z);
        }

        public static Vector3 CalculatePositionBehind(Vector3 position, float rotation, float distanceToMove = 2.0f)
        {
            return CalculatePositionAround(position, rotation, (float)Math.PI, distanceToMove);
        }

        public static float ClampAngles(float rotation)
        {
            float rMax = (float)Math.PI * 2.0f;

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

        public static float GetFacingAngle2D(Vector3 position, Vector3 targetPosition)
                    => ClampAngles(Convert.ToSingle(Math.Atan2(targetPosition.Y - position.Y, targetPosition.X - position.X)));

        public static bool IsFacing(Vector3 position, float rotation, Vector3 targetPosition, float maxAngleDiff = 1.5f)
        {
            float facingAngle = GetFacingAngle2D(position, targetPosition);
            float angleDiff = ClampAngles(facingAngle - rotation);

            return angleDiff < maxAngleDiff;
        }
    }
}