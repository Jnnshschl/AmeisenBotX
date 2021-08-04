using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AmeisenBotX.Core.Keyboard
{
    /// <summary>
    /// This is a global keyboard hook
    /// </summary>
    public class KeyboardHook : IDisposable
    {
        private LowLevelKeyboardProc _hookProc;
        private IntPtr _hookPtr;

        #region Constructor

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public KeyboardHook()
        {
            // Set
            this._hookProc = this.LowLevelKeyboardCallback;
        }

        #endregion

        #region Public

        /// <summary>
        /// Disables the keyboard hook.
        /// </summary>
        public void Disable()
        {
            if (this._hookPtr == IntPtr.Zero)
            {
                // Skip
                return;
            }

            // Un-hook
            KeyboardHook.UnhookWindowsHookEx(hHook: this._hookPtr);
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            // Disable
            this.Disable();
        }

        /// <summary>
        /// Enables the keyboard hook.
        /// </summary>
        public void Enable()
        {
            if (this._hookPtr != IntPtr.Zero)
            {
                // Skip
                return;
            }

            // Create hook
            this._hookPtr = KeyboardHook.SetWindowsHookEx(idHook: HookType.WhKeyboardLl,
                                                                                        lpfn: this._hookProc,
                                                                                        hMod: KeyboardHook.GetModuleHandle(lpModuleName: Process.GetCurrentProcess().MainModule.ModuleName),
                                                                                        dwThreadId: 0);

            // Invalid?
            if (this._hookPtr == IntPtr.Zero)
            {
                // Throw
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public delegate void KeyDownEventDelegate(KeyboardHookEventArgs e);

        public delegate void KeyUpEventDelegate(KeyboardHookEventArgs e);

        /// <summary>
        /// Gets called if a key is pressed.
        /// </summary>
        public KeyDownEventDelegate OnPressed;

        /// <summary>
        /// Gets called if a key is released.
        /// </summary>
        public KeyUpEventDelegate OnReleased;

        #endregion

        #region Private

        private int LowLevelKeyboardCallback(int nCode, IntPtr wParam, ref LowLevelKeyboardInputEvent lParam)
        {
            int result = 0;
            bool handled = false;

            int wParamValue = wParam.ToInt32();
            if (Enum.IsDefined(typeof(KeyboardState), wParamValue))
            {
                // Get state
                KeyboardState keyboardState = (KeyboardState)wParamValue;

                // Key down?
                if (keyboardState == KeyboardState.KeyDown
                    || keyboardState == KeyboardState.SysKeyDown)
                {
                    // Create new event args
                    KeyboardHookEventArgs keyboardHookEventArgs = new KeyboardHookEventArgs(keyboardInput: lParam);

                    // Signal
                    this.OnPressed?.Invoke(keyboardHookEventArgs);

                    // Set
                    handled = keyboardHookEventArgs.Handled;
                }

                // Key up?
                if (keyboardState == KeyboardState.KeyUp
                    || keyboardState == KeyboardState.SysKeyUp)
                {
                    // Create new event args
                    KeyboardHookEventArgs keyboardHookEventArgs = new KeyboardHookEventArgs(keyboardInput: lParam);

                    // Signal
                    this.OnReleased?.Invoke(new KeyboardHookEventArgs(keyboardInput: lParam));

                    // Set
                    handled = keyboardHookEventArgs.Handled;
                }
            }

            if (handled)
            {
                // Return
                return 1;
            }

            // Return
            return KeyboardHook.CallNextHookEx(hHook: IntPtr.Zero,
                                                                    nCode: nCode,
                                                                    wParam: wParam,
                                                                    lParam: ref lParam);
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

        #endregion

        #region Win32

        private delegate int LowLevelKeyboardProc(int nCode, IntPtr wParam, ref LowLevelKeyboardInputEvent lParam);


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

        #endregion
    }
}
