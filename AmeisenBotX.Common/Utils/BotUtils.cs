using AmeisenBotX.Common.Math;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AmeisenBotX.Common.Utils
{
    public static class BotUtils
    {
        private const uint WM_KEYDOWN = 0x100;
        private const uint WM_KEYUP = 0x101;

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
        public static string FastRandomString()
        {
            return Path.GetRandomFileName().Replace(".", string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FastRandomStringOnlyLetters()
        {
            return new string(FastRandomString().Where(c => c != '-' && (c < '0' || c > '9')).ToArray());
        }

        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput))
            {
                return false;
            }

            strInput = strInput.Trim();

            if ((strInput.StartsWith("{") && strInput.EndsWith("}"))
                || (strInput.StartsWith("[") && strInput.EndsWith("]")))
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
        public static Vector3 MoveAhead(Vector3 origin, Vector3 targetPosition, float offset)
        {
            return MoveAhead(targetPosition, BotMath.GetFacingAngle(origin, targetPosition), offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 MoveAhead(Vector3 targetPosition, float rotation, float offset)
        {
            return BotMath.CalculatePositionAround(targetPosition, rotation, 0f, offset);
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
            if (string.IsNullOrWhiteSpace(input))
            {
                return (string.Empty, string.Empty);
            }

            string returnValueName = string.Empty;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendKey(IntPtr windowHandle, IntPtr key, int minDelay = 20, int maxDelay = 40)
        {
            SendMessage(windowHandle, WM_KEYDOWN, key, IntPtr.Zero);
            Task.Delay(new Random().Next(minDelay, maxDelay)).Wait();
            SendMessage(windowHandle, WM_KEYUP, key, IntPtr.Zero);
        }

        [DllImport("user32", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr windowHandle, uint msg, IntPtr param, IntPtr parameter);
    }
}