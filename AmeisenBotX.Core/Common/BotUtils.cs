using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.WowObject;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AmeisenBotX.Core.Common
{
    public class BotUtils
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static void SendKey(IntPtr windowHandle, IntPtr vKey, int minDelay = 20, int maxDelay = 40)
        {
            SendMessage(windowHandle, 0x100, vKey, new IntPtr(0));
            Thread.Sleep(new Random().Next(minDelay, maxDelay)); // make it look more human-like :^)
            SendMessage(windowHandle, 0x101, vKey, new IntPtr(0));
        }

        public static void SendKeyShift(IntPtr windowHandle, IntPtr vKey, bool shift)
        {
            if (shift) PostMessage(windowHandle, 0x0100, new IntPtr(0x10), new IntPtr(0));
            PostMessage(windowHandle, 0x0102, vKey, new IntPtr(0));
            if (shift) PostMessage(windowHandle, 0x0101, new IntPtr(0x10), new IntPtr(0));
        }

        public static bool IsValidUnit(WowUnit unit)
        {
            if (unit == null
                || unit.Health <= 0
                || unit.IsNotAttackable)
                return false;
            return true;
        }

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

        public static bool IsMeeleeClass(WowClass wowClass)
        {
            switch (wowClass)
            {
                case WowClass.DeathKnight: return true;
                case WowClass.Paladin: return true;
                case WowClass.Rogue: return true;
                case WowClass.Warrior: return true;

                // special case, need to check for owl
                case WowClass.Druid: return true;

                // special case, need to check for enhancement
                case WowClass.Shaman: return false;

                // special case, need to check for survival
                case WowClass.Hunter: return false;



                default:
                    return false;
            }
        }
    }
}