using AmeisenBotX.Core.Data.Objects.Structs;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Drawing;
using Rect = AmeisenBotX.Memory.Win32.Rect;

namespace AmeisenBotX.Overlay.Utils
{
    public static class OverlayMath
    {
        public const float DEG_TO_RAD = 0.01745329251f;

        public static bool WorldToScreen(Rect clientRect, CameraInfo cameraInfo, Vector3 position, out Point screenCoordinates)
        {
            Vector3 diff = new Vector3(position) - cameraInfo.Pos;
            screenCoordinates = new Point(0, 0);

            if ((diff * cameraInfo.ViewMatrix.FirstCol) < Vector3.Zero)
            {
                return false;
            }

            Vector3 view = diff * cameraInfo.ViewMatrix.Inverse();
            Vector3 cam = new Vector3(-view.Y, -view.Z, view.X);

            float windowWidth = (clientRect.Right - clientRect.Left);
            float windowHeight = (clientRect.Bottom - clientRect.Top);

            float screenX = windowWidth / 2.0f;
            float screenY = windowHeight / 2.0f;

            float tmpX = screenX / (float)Math.Tan((cameraInfo.Fov * 180) * DEG_TO_RAD);
            float tmpY = screenY / (float)Math.Tan((cameraInfo.Fov * 180) * DEG_TO_RAD);

            screenCoordinates = new Point
            {
                X = (int)(screenX + cam.X * tmpX / cam.Z),
                Y = (int)(screenY + cam.Y * tmpY / cam.Z)
            };

            return screenCoordinates.X > 0 && screenCoordinates.Y > 0 && screenCoordinates.X < windowWidth && screenCoordinates.Y < windowHeight;
        }
    }
}