using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Common
{
    public static class BotUtils
    {
#pragma warning disable IDE0051
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
#pragma warning restore IDE0051

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ByteArrayToString(byte[] bytes)
        {
            return bytes != null ? BitConverter.ToString(bytes).Replace("-", " ", StringComparison.OrdinalIgnoreCase) : "null";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void HoldKey(IntPtr windowHandle, IntPtr key)
        {
            SendMessage(windowHandle, WM_KEYDOWN, key, new IntPtr(0));
        }

        public static string CleanString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            StringBuilder sb = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; ++i)
            {
                char c = input[i];
                if (c != '\n' && c != '\r' && c != '\t')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Replace the variables in a LUA string to make them less obvious.
        ///
        /// Mark the variable places using this format: "{v:0}=1" ({v:0} will be replaced with a random string)
        /// Name the next {v:1} the next {v:2} and so on.
        ///
        /// The first variable will always be the return variable name!
        /// </summary>
        /// <param name="input">LUA string</param>
        /// <returns>(LUA string, return variable name)</returns>
        public static (string, string) ObfuscateLua(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (string.Empty, string.Empty);

            string returnValueName = "";

            for (int i = 0; ; ++i)
            {
                string symbol = $"{{v:{i}}}";
                if (input.Contains(symbol, StringComparison.OrdinalIgnoreCase))
                {
                    string newValueName = FastRandomStringOnlyLetters();
                    input = input.Replace(symbol, newValueName, StringComparison.OrdinalIgnoreCase);

                    if (i == 0)
                    {
                        returnValueName = newValueName;
                    }
                }
                else
                {
                    break;
                }
            }

            return (input, returnValueName);
        }

        public static string GenerateUniqueString(int byteCount)
        {
            using RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            byte[] bytes = new byte[byteCount];
            rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FastRandomStringOnlyLetters()
        {
            return new string(FastRandomString().Where(c => c != '-' && (c < '0' || c > '9')).ToArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FastRandomString()
        {
            return Path.GetRandomFileName().Replace(".", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetColorByQuality(ItemQuality itemQuality)
        {
            return itemQuality switch
            {
                ItemQuality.Unique => "#00ccff",
                ItemQuality.Poor => "#9d9d9d",
                ItemQuality.Common => "#ffffff",
                ItemQuality.Uncommon => "#1eff00",
                ItemQuality.Rare => "#0070dd",
                ItemQuality.Epic => "#a335ee",
                ItemQuality.Legendary => "#ff8000",
                ItemQuality.Artifact => "#e6cc80",
                _ => "#ffffff",
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Capitalize(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.First().ToString().ToUpper(CultureInfo.InvariantCulture) + input.Substring(1);
        }

        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) return false;

            strInput = strInput.Trim();

            if ((strInput.StartsWith("{", StringComparison.OrdinalIgnoreCase) && strInput.EndsWith("}", StringComparison.OrdinalIgnoreCase))
                || (strInput.StartsWith("[", StringComparison.OrdinalIgnoreCase) && strInput.EndsWith("]", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    JToken obj = JToken.Parse(strInput);
                    return true;
                }
                catch { }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPositionInsideAoeSpell(Vector3 position, IEnumerable<WowDynobject> wowDynobjects)
        {
            return wowDynobjects.Any(e => e.Position.GetDistance(position) < e.Radius + 3.0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidUnit(WowUnit unit)
        {
            return unit != null
                && !unit.IsNotAttackable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 MoveAhead(Vector3 origin, Vector3 targetPosition, float offset)
        {
            return MoveAhead(targetPosition, BotMath.GetFacingAngle2D(origin, targetPosition), offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 MoveAhead(Vector3 targetPosition, float rotation, float offset)
        {
            return BotMath.CalculatePositionAround(targetPosition, rotation, 0f, offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RealeaseKey(IntPtr windowHandle, IntPtr key)
        {
            SendMessage(windowHandle, WM_KEYUP, key, new IntPtr(0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendKey(IntPtr windowHandle, IntPtr key, int minDelay = 20, int maxDelay = 40)
        {
            SendMessage(windowHandle, WM_KEYDOWN, key, new IntPtr(0));
            Task.Delay(new Random().Next(minDelay, maxDelay)).Wait();
            SendMessage(windowHandle, WM_KEYUP, key, new IntPtr(0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendKeyShift(IntPtr windowHandle, IntPtr key, bool shift)
        {
            if (shift)
            {
                PostMessage(windowHandle, WM_KEYDOWN, new IntPtr((int)VirtualKey.VKSHIFT), new IntPtr(0));
            }

            PostMessage(windowHandle, WM_KEYPRESS, key, new IntPtr(0));

            if (shift)
            {
                PostMessage(windowHandle, WM_KEYUP, new IntPtr((int)VirtualKey.VKSHIFT), new IntPtr(0));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendMouseMovement(IntPtr windowHandle, short x, short y)
        {
            SendMessage(windowHandle, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(x, y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendMouseMovementHoldLeft(IntPtr windowHandle, short x, short y)
        {
            SendMessage(windowHandle, WM_MOUSEMOVE, new IntPtr(WM_LBUTTONDOWN), MakeLParam(x, y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendMouseMovementHoldRight(IntPtr windowHandle, short x, short y)
        {
            SendMessage(windowHandle, WM_MOUSEMOVE, new IntPtr(WM_RBUTTONDOWN), MakeLParam(x, y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr MakeLParam(int p, int p2)
        {
            return new IntPtr((p2 << 16) | (p & 0xFFFF));
        }

        [DllImport("user32", SetLastError = true)]
        private static extern bool PostMessage(IntPtr windowHandle, uint msg, IntPtr param, IntPtr parameter);

        [DllImport("user32", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr windowHandle, uint msg, IntPtr param, IntPtr parameter);
    }
}