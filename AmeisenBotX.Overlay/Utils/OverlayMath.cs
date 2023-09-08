using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Memory;
using AmeisenBotX.Wow.Objects.Raw;
using System;
using System.Drawing;

namespace AmeisenBotX.Overlay.Utils
{
    public static class OverlayMath
    {
        public const float DEG_TO_RAD = MathF.PI / 180.0f;

        /// <summary>
        /// Transform world coordinates to screen coordinates.
        /// </summary>
        /// <param name="clientRect">Window size</param>
        /// <param name="cameraInfo">Game camera info</param>
        /// <param name="position">World position</param>
        /// <param name="screenCoordinates">Screen Position</param>
        /// <returns>True when coordinates are on th window, fals eif not</returns>
        public static bool WorldToScreen(Rect clientRect, RawCameraInfo cameraInfo, Vector3 position, out Point screenCoordinates)
        {
            Vector3 diff = position - cameraInfo.Pos;
            Vector3 view = diff * cameraInfo.ViewMatrix.Inverse();

            float windowWidth = clientRect.Right - clientRect.Left;
            float windowHeight = clientRect.Bottom - clientRect.Top;

            float screenX = windowWidth / 2.0f;
            float screenY = windowHeight / 2.0f;
            float screenF = windowWidth / windowHeight;

            float tmpX = screenX / MathF.Tan((screenF * (screenF >= 1.6f ? 55.0f : 44.0f) / 2.0f) * DEG_TO_RAD);
            float tmpY = screenY / MathF.Tan((screenF * 35.0f / 2.0f) * DEG_TO_RAD);

            screenCoordinates = new()
            {
                X = (int)MathF.Abs(screenX + (-view.Y * tmpX / view.X)),
                Y = (int)MathF.Abs(screenY + (-view.Z * tmpY / view.X))
            };

            return screenCoordinates.X > 0 && screenCoordinates.Y > 0 && screenCoordinates.X < windowWidth && screenCoordinates.Y < windowHeight;
        }
    }
}