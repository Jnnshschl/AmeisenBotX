using AmeisenBotX.Common.Keyboard.Enums;
using AmeisenBotX.Common.Keyboard.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Common.Keyboard
{
    /// <summary>
    /// This is a global keyboard hook used to manage hotkeys.
    /// </summary>
    public partial class KeyboardHook
    {
        public KeyboardHook()
        {
            Hotkeys = [];
            KeyboardProc = LowLevelKeyboardCallback;
        }

        private delegate int LowLevelKeyboardProc(int nCode, nint wParam, ref LowLevelKeyboardInput lParam);

        private nint HookPtr { get; set; }

        private List<(KeyCode, KeyCode, Action)> Hotkeys { get; }

        private LowLevelKeyboardProc KeyboardProc { get; }

        /// <summary>
        /// Register a new Hotkey.
        /// </summary>
        /// <param name="key">Main key to press</param>
        /// <param name="modifier">Modifier key, example: CTRL, ALT, ...</param>
        /// <param name="callback">Action to run when the key is pressed</param>
        public void AddHotkey(KeyCode key, KeyCode modifier, Action callback)
        {
            Hotkeys.Add((key, modifier, callback));
        }

        /// <summary>
        /// Register a new Hotkey without a modifier.
        /// </summary>
        /// <param name="key">Main key to press</param>
        /// <param name="callback">Action to run when the key is pressed</param>
        public void AddHotkey(KeyCode key, Action callback)
        {
            Hotkeys.Add((key, KeyCode.None, callback));
        }

        public void Clear()
        {
            Hotkeys.Clear();
        }

        public void Disable()
        {
            if (HookPtr != nint.Zero)
            {
                _ = UnhookWindowsHookEx(HookPtr);
            }
        }

        public void Enable()
        {
            if (HookPtr == nint.Zero)
            {
                HookPtr = SetWindowsHookEx
                (
                    13,
                    KeyboardProc,
                    GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
                    0
                );
            }
        }

        [LibraryImport("user32")]
        private static partial int CallNextHookEx(nint hHook, int nCode, nint wParam, ref LowLevelKeyboardInput lParam);

        [LibraryImport("user32")]
        private static partial short GetKeyState(KeyCode nVirtKey);

        [LibraryImport("kernel32", EntryPoint = "GetModuleHandleA", StringMarshalling = StringMarshalling.Utf16)]
        private static partial nint GetModuleHandle(string lpModuleName);

        [LibraryImport("user32", EntryPoint = "SetWindowsHookExA")]
        private static partial nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, int dwThreadId);

        [LibraryImport("user32")]
        private static partial int UnhookWindowsHookEx(nint hHook);

        private int LowLevelKeyboardCallback(int nCode, nint wParam, ref LowLevelKeyboardInput lParam)
        {
            int wParamValue = wParam.ToInt32();

            if (Enum.IsDefined(typeof(KeyboardState), wParamValue))
            {
                if ((KeyboardState)wParamValue is KeyboardState.KeyDown or KeyboardState.SysKeyDown)
                {
                    foreach ((KeyCode key, KeyCode mod, Action callback) in Hotkeys)
                    {
                        if (lParam.VirtualCode == key && (mod == KeyCode.None || (GetKeyState(mod) & 0x8000) > 0))
                        {
                            callback?.Invoke();
                        }
                    }
                }
            }

            return CallNextHookEx(nint.Zero, nCode, wParam, ref lParam);
        }
    }
}