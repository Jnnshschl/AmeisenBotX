using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Keyboard
{
    /// <summary>
    /// This is a global keyboard hook
    /// </summary>
    public class KeyboardHook : IDisposable
    {
        /// <summary>
        /// Gets called if a key is pressed.
        /// </summary>
        public KeyDownEventDelegate OnPressed;

        /// <summary>
        /// Gets called if a key is released.
        /// </summary>
        public KeyUpEventDelegate OnReleased;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public KeyboardHook()
        {
            KeyboardProc = LowLevelKeyboardCallback;
        }

        public delegate void KeyDownEventDelegate(KeyboardHookEventArgs e);

        public delegate void KeyUpEventDelegate(KeyboardHookEventArgs e);

        private delegate int LowLevelKeyboardProc(int nCode, IntPtr wParam, ref LowLevelKeyboardInputEvent lParam);

        private enum HookType
        {
            WhJournalrecord = 0,
            WhJournalplayback = 1,
            WhKeyboard = 2,
            WhGetmessage = 3,
            WhCallwndproc = 4,
            WhCbt = 5,
            WhSysmsgfilter = 6,
            WhMouse = 7,
            WhHardware = 8,
            WhDebug = 9,
            WhShell = 10,
            WhForegroundidle = 11,
            WhCallwndprocret = 12,
            WhKeyboardLl = 13,
            WhMouseLl = 14
        }

        /// <summary>
        /// Holds the keyboard states
        /// </summary>
        private enum KeyboardState
        {
            KeyDown = 0x0100,
            KeyUp = 0x0101,
            SysKeyDown = 0x0104,
            SysKeyUp = 0x0105
        }

        private IntPtr HookPtr { get; set; }

        private LowLevelKeyboardProc KeyboardProc { get; }

        /// <summary>
        /// Disables the keyboard hook.
        /// </summary>
        public void Disable()
        {
            if (HookPtr == IntPtr.Zero)
            {
                return;
            }

            UnhookWindowsHookEx(hHook: HookPtr);
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            Disable();
        }

        /// <summary>
        /// Enables the keyboard hook.
        /// </summary>
        public void Enable()
        {
            if (HookPtr != IntPtr.Zero)
            {
                return;
            }

            HookPtr = SetWindowsHookEx
            (
                HookType.WhKeyboardLl,
                KeyboardProc,
                GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
                0
            );

            if (HookPtr == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        ///     Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call this
        ///     function either before or after processing the hook information.
        /// </summary>
        /// <param name="hhk">This parameter is ignored.</param>
        /// <param name="nCode">
        ///     The hook code passed to the current hook procedure. The next hook procedure uses this code to
        ///     determine how to process the hook information.
        /// </param>
        /// <param name="wParam">
        ///     The wParam value passed to the current hook procedure. The meaning of this parameter depends on
        ///     the type of hook associated with the current hook chain.
        /// </param>
        /// <param name="lParam">
        ///     The lParam value passed to the current hook procedure. The meaning of this parameter depends on
        ///     the type of hook associated with the current hook chain.
        /// </param>
        /// <returns>
        ///     This value is returned by the next hook procedure in the chain. The current hook procedure must also return
        ///     this value. The meaning of the return value depends on the hook type. For more information, see the descriptions of
        ///     the individual hook procedures.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, ref LowLevelKeyboardInputEvent lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        ///     Installs an application-defined hook procedure into a hook chain.
        /// </summary>
        /// <param name="idHook">The type of hook procedure to be installed.</param>
        /// <param name="lpfn">Reference to the hook callback method.</param>
        /// <param name="hMod">
        ///     A handle to the DLL containing the hook procedure pointed to by the lpfn parameter. The hMod
        ///     parameter must be set to NULL if the dwThreadId parameter specifies a thread created by the current process and if
        ///     the hook procedure is within the code associated with the current process.
        /// </param>
        /// <param name="dwThreadId">
        ///     The identifier of the thread with which the hook procedure is to be associated. If this
        ///     parameter is zero, the hook procedure is associated with all existing threads running in the same desktop as the
        ///     calling thread.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is the handle to the hook procedure. If the function fails, the
        ///     return value is NULL. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(HookType idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, int dwThreadId);

        /// <summary>
        ///     Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.
        /// </summary>
        /// <param name="hhk">
        ///     A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to
        ///     SetWindowsHookEx.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get
        ///     extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll")]
        private static extern int UnhookWindowsHookEx(IntPtr hHook);

        private int LowLevelKeyboardCallback(int nCode, IntPtr wParam, ref LowLevelKeyboardInputEvent lParam)
        {
            bool handled = false;
            int wParamValue = wParam.ToInt32();

            if (Enum.IsDefined(typeof(KeyboardState), wParamValue))
            {
                KeyboardState keyboardState = (KeyboardState)wParamValue;

                if (keyboardState == KeyboardState.KeyDown
                    || keyboardState == KeyboardState.SysKeyDown)
                {
                    KeyboardHookEventArgs keyboardHookEventArgs = new(keyboardInput: lParam);
                    OnPressed?.Invoke(keyboardHookEventArgs);
                    handled = keyboardHookEventArgs.Handled;
                }

                if (keyboardState == KeyboardState.KeyUp
                    || keyboardState == KeyboardState.SysKeyUp)
                {
                    KeyboardHookEventArgs keyboardHookEventArgs = new(keyboardInput: lParam);
                    OnReleased?.Invoke(new KeyboardHookEventArgs(keyboardInput: lParam));
                    handled = keyboardHookEventArgs.Handled;
                }
            }

            if (handled)
            {
                return 1;
            }

            return CallNextHookEx(IntPtr.Zero, nCode, wParam, ref lParam);
        }
    }
}