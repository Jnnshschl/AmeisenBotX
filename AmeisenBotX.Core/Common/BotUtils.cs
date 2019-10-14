using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace AmeisenBotX.Core.Common
{
    public class BotUtils
    {
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

        public static bool IsValidUnit(WowUnit unit)
        {
            return unit == null
                || unit.Health <= 0
                || unit.IsNotAttackable;
        }

        public static void SendKey(IntPtr windowHandle, IntPtr key, int minDelay = 20, int maxDelay = 40)
        {
            SendMessage(windowHandle, 0x100, key, new IntPtr(0));
            Thread.Sleep(new Random().Next(minDelay, maxDelay));
            SendMessage(windowHandle, 0x101, key, new IntPtr(0));
        }

        public static bool IsPositionInsideAoeSpell(Vector3 position, List<WowDynobject> wowDynobjects)
            => wowDynobjects.Any(e => e.Position.GetDistance2D(position) < e.Radius + 1);

        public static void SendKeyShift(IntPtr windowHandle, IntPtr key, bool shift)
        {
            if (shift)
            {
                PostMessage(windowHandle, 0x0100, new IntPtr(0x10), new IntPtr(0));
            }

            PostMessage(windowHandle, 0x0102, key, new IntPtr(0));

            if (shift)
            {
                PostMessage(windowHandle, 0x0101, new IntPtr(0x10), new IntPtr(0));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr windowHandle, uint msg, IntPtr param, IntPtr parameter);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr windowHandle, int msg, IntPtr param, IntPtr parameter);
    }
}