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