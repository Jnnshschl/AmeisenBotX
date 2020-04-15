using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AmeisenBotX.Core.Common
{
    public class BotUtils
    {
        private const uint MK_CONTROL = 0x8;
        private const uint MK_LBUTTON = 0x1;
        private const uint MK_MBUTTON = 0x10;
        private const uint MK_RBUTTON = 0x2;
        private const uint MK_SHIFT = 0x4;
        private const uint MK_XBUTTON1 = 0x20;
        private const uint MK_XBUTTON2 = 0x40;
        private const uint WM_KEYDOWN = 0x100;
        private const uint WM_KEYPRESS = 0x102;
        private const uint WM_KEYUP = 0x101;
        private const uint WM_LBUTTONDBLCLK = 0x203;
        private const uint WM_LBUTTONDOWN = 0x201;
        private const uint WM_LBUTTONUP = 0x202;
        private const uint WM_MOUSEMOVE = 0x200;
        private const uint WM_RBUTTONDBLCLK = 0x206;
        private const uint WM_RBUTTONDOWN = 0x204;
        private const uint WM_RBUTTONUP = 0x205;

        public static string BigValueToString(double value)
        {
            if (value >= 100000000)
            {
                return $"{(int)value / 1000000}M";
            }
            else if (value >= 100000)
            {
                return $"{(int)value / 1000}K";
            }

            return $"{value}";
        }

        public static string ByteArrayToString(byte[] bytes)
            => BitConverter.ToString(bytes).Replace("-", " ");

        public static string Capitalize(string input)
            => input.First().ToString().ToUpper() + input.Substring(1);

        public static void HoldKey(IntPtr windowHandle, IntPtr key)
        {
            SendMessage(windowHandle, WM_KEYDOWN, key, new IntPtr(0));
        }

        public static bool IsPositionInsideAoeSpell(Vector3 position, List<WowDynobject> wowDynobjects)
            => wowDynobjects.Any(e => e.Position.GetDistance(position) < e.Radius + 1);

        public static bool IsValidUnit(WowUnit unit)
        {
            return unit != null
                && !unit.IsNotAttackable;
        }

        public static Vector3 MoveAhead(float rotation, Vector3 targetPosition, double offset)
        {
            double x = targetPosition.X + (Math.Cos(rotation) * offset);
            double y = targetPosition.Y + (Math.Sin(rotation) * offset);

            return new Vector3()
            {
                X = Convert.ToSingle(x),
                Y = Convert.ToSingle(y),
                Z = targetPosition.Z
            };
        }

        public static void RealeaseKey(IntPtr windowHandle, IntPtr key)
        {
            SendMessage(windowHandle, WM_KEYUP, key, new IntPtr(0));
        }

        public static void SendKey(IntPtr windowHandle, IntPtr key, int minDelay = 20, int maxDelay = 40)
        {
            SendMessage(windowHandle, WM_KEYDOWN, key, new IntPtr(0));
            Thread.Sleep(new Random().Next(minDelay, maxDelay));
            SendMessage(windowHandle, WM_KEYUP, key, new IntPtr(0));
        }

        public static void SendKeyShift(IntPtr windowHandle, IntPtr key, bool shift)
        {
            if (shift)
            {
                PostMessage(windowHandle, WM_KEYDOWN, new IntPtr((int)VirtualKeys.VK_SHIFT), new IntPtr(0));
            }

            PostMessage(windowHandle, WM_KEYPRESS, key, new IntPtr(0));

            if (shift)
            {
                PostMessage(windowHandle, WM_KEYUP, new IntPtr((int)VirtualKeys.VK_SHIFT), new IntPtr(0));
            }
        }

        public static void SendMouseMovement(IntPtr windowHandle, short x, short y)
        {
            SendMessage(windowHandle, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(x, y));
        }

        public static void SendMouseMovementHoldLeft(IntPtr windowHandle, short x, short y)
        {
            SendMessage(windowHandle, WM_MOUSEMOVE, new IntPtr(WM_LBUTTONDOWN), MakeLParam(x, y));
        }

        public static void SendMouseMovementHoldRight(IntPtr windowHandle, short x, short y)
        {
            SendMessage(windowHandle, WM_MOUSEMOVE, new IntPtr(WM_RBUTTONDOWN), MakeLParam(x, y));
        }

        private static IntPtr MakeLParam(int p, int p2)
        {
            return new IntPtr((p2 << 16) | (p & 0xFFFF));
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr windowHandle, uint msg, IntPtr param, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr windowHandle, uint msg, IntPtr param, IntPtr parameter);
    }
}