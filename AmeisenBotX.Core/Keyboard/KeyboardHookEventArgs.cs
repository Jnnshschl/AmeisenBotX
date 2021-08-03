using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Keyboard
{
    public class KeyboardHookEventArgs
    {
        private Keys _key;
        private HashSet<VirtualKeyStates> _alts;

        #region Constructor

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public KeyboardHookEventArgs(LowLevelKeyboardInputEvent keyboardInput)
        {
            // Read key
            Keys key = (Keys)keyboardInput.VirtualCode;

            // Store
            this._key = this.AltKeyList.Contains(this._key) ? Keys.None : key;
            this._alts = this.GetPressedAltKeys((VirtualKeyStates.VK_LALT, Keys.LMenu),
                                                                (VirtualKeyStates.VK_RALT, Keys.RMenu),
                                                                (VirtualKeyStates.VK_LCONTROL, Keys.LControlKey),
                                                                (VirtualKeyStates.VK_RCONTROL, Keys.RControlKey),
                                                                (VirtualKeyStates.VK_LSHIFT, Keys.LShiftKey),
                                                                (VirtualKeyStates.VK_RSHIFT, Keys.RShiftKey),
                                                                (VirtualKeyStates.VK_LWIN, Keys.LWin),
                                                                (VirtualKeyStates.VK_RWIN, Keys.RWin));
        }

        #endregion

        #region Public

        /// <summary>
        /// Gets the key.
        /// </summary>
        public Keys Key => this._key;

        /// <summary>
        /// Gets the alt key.
        /// </summary>
        public HashSet<VirtualKeyStates> Alt => this._alts;

        // Indicates if the event is handled or not.
        public bool Handled { get; set; }

        #endregion

        #region Private

        private HashSet<VirtualKeyStates> GetPressedAltKeys(params (VirtualKeyStates, Keys)[] keys)
        {
            // Create alt key set
            HashSet<VirtualKeyStates> altKeys = new HashSet<VirtualKeyStates>();

            // Process given keys
            foreach ((VirtualKeyStates, Keys) keySet in keys)
            {
                if ((Convert.ToBoolean(GetKeyState(keySet.Item1) & KEY_PRESSED)
                    || Key == keySet.Item2)
                    && !altKeys.Contains(keySet.Item1))
                {
                    // Add
                    altKeys.Add(keySet.Item1);
                }
            }

            // Return
            return altKeys;
        }

        [DllImport("user32.dll")]
        private static extern short GetKeyState(VirtualKeyStates nVirtKey);

        private const int KEY_PRESSED = 0x8000;

        private readonly HashSet<Keys> AltKeyList = new HashSet<Keys>()
        {
            Keys.LMenu,
            Keys.RMenu,
            Keys.LControlKey,
            Keys.RControlKey,
            Keys.LShiftKey,
            Keys.RShiftKey,
            Keys.LWin,
            Keys.RWin
        };

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelKeyboardInputEvent
    {
        /// <summary>
        /// A virtual-key code. The code must be a value in the range 1 to 254.
        /// </summary>
        public int VirtualCode;

        /// <summary>
        /// A hardware scan code for the key. 
        /// </summary>
        public int HardwareScanCode;

        /// <summary>
        /// The extended-key flag, event-injected Flags, context code, and transition-state flag. This member is specified as follows. An application can use the following values to test the keystroke Flags. Testing LLKHF_INJECTED (bit 4) will tell you whether the event was injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you whether or not the event was injected from a process running at lower integrity level.
        /// </summary>
        public int Flags;

        /// <summary>
        /// The time stamp stamp for this message, equivalent to what GetMessageTime would return for this message.
        /// </summary>
        public int TimeStamp;

        /// <summary>
        /// Additional information associated with the message. 
        /// </summary>
        public IntPtr AdditionalInformation;
    }

    public enum VirtualKeyStates
    {
        VK_LWIN = 0x5B,
        VK_RWIN = 0x5C,
        VK_LSHIFT = 0xA0,
        VK_RSHIFT = 0xA1,
        VK_LCONTROL = 0xA2,
        VK_RCONTROL = 0xA3,
        VK_LALT = 0xA4, //aka VK_LMENU
        VK_RALT = 0xA5 //aka VK_RMENU
    }

    /// <summary>
    /// The key codes and modifiers list.
    /// </summary>
    [Flags]
    public enum Keys
    {
        A = 0x41,
        Add = 0x6b,
        Alt = 0x40000,
        Apps = 0x5d,
        Attn = 0xf6,
        B = 0x42,
        Back = 8,
        BrowserBack = 0xa6,
        BrowserFavorites = 0xab,
        BrowserForward = 0xa7,
        BrowserHome = 0xac,
        BrowserRefresh = 0xa8,
        BrowserSearch = 170,
        BrowserStop = 0xa9,
        C = 0x43,
        Cancel = 3,
        Capital = 20,
        CapsLock = 20,
        Clear = 12,
        Control = 0x20000,
        ControlKey = 0x11,
        Crsel = 0xf7,
        D = 0x44,
        D0 = 0x30,
        D1 = 0x31,
        D2 = 50,
        D3 = 0x33,
        D4 = 0x34,
        D5 = 0x35,
        D6 = 0x36,
        D7 = 0x37,
        D8 = 0x38,
        D9 = 0x39,
        Decimal = 110,
        Delete = 0x2e,
        Divide = 0x6f,
        Down = 40,
        E = 0x45,
        End = 0x23,
        Enter = 13,
        EraseEof = 0xf9,
        Escape = 0x1b,
        Execute = 0x2b,
        Exsel = 0xf8,
        F = 70,
        F1 = 0x70,
        F10 = 0x79,
        F11 = 0x7a,
        F12 = 0x7b,
        F13 = 0x7c,
        F14 = 0x7d,
        F15 = 0x7e,
        F16 = 0x7f,
        F17 = 0x80,
        F18 = 0x81,
        F19 = 130,
        F2 = 0x71,
        F20 = 0x83,
        F21 = 0x84,
        F22 = 0x85,
        F23 = 0x86,
        F24 = 0x87,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 120,
        FinalMode = 0x18,
        G = 0x47,
        H = 0x48,
        HanguelMode = 0x15,
        HangulMode = 0x15,
        HanjaMode = 0x19,
        Help = 0x2f,
        Home = 0x24,
        I = 0x49,
        ImeAccept = 30,
        ImeAceept = 30,
        ImeConvert = 0x1c,
        ImeModeChange = 0x1f,
        ImeNonconvert = 0x1d,
        Insert = 0x2d,
        J = 0x4a,
        JunjaMode = 0x17,
        K = 0x4b,
        KanaMode = 0x15,
        KanjiMode = 0x19,
        KeyCode = 0xffff,
        L = 0x4c,
        LaunchApplication1 = 0xb6,
        LaunchApplication2 = 0xb7,
        LaunchMail = 180,
        LButton = 1,
        LControlKey = 0xa2,
        Left = 0x25,
        LineFeed = 10,
        LMenu = 0xa4,
        LShiftKey = 160,
        LWin = 0x5b,
        M = 0x4d,
        MButton = 4,
        MediaNextTrack = 0xb0,
        MediaPlayPause = 0xb3,
        MediaPreviousTrack = 0xb1,
        MediaStop = 0xb2,
        Menu = 0x12,
        Modifiers = -65536,
        Multiply = 0x6a,
        N = 0x4e,
        Next = 0x22,
        NoName = 0xfc,
        None = 0,
        NumLock = 0x90,
        NumPad0 = 0x60,
        NumPad1 = 0x61,
        NumPad2 = 0x62,
        NumPad3 = 0x63,
        NumPad4 = 100,
        NumPad5 = 0x65,
        NumPad6 = 0x66,
        NumPad7 = 0x67,
        NumPad8 = 0x68,
        NumPad9 = 0x69,
        O = 0x4f,
        Oem1 = 0xba,
        Oem102 = 0xe2,
        Oem2 = 0xbf,
        Oem3 = 0xc0,
        Oem4 = 0xdb,
        Oem5 = 220,
        Oem6 = 0xdd,
        Oem7 = 0xde,
        Oem8 = 0xdf,
        OemBackslash = 0xe2,
        OemClear = 0xfe,
        OemCloseBrackets = 0xdd,
        Oemcomma = 0xbc,
        OemMinus = 0xbd,
        OemOpenBrackets = 0xdb,
        OemPeriod = 190,
        OemPipe = 220,
        Oemplus = 0xbb,
        OemQuestion = 0xbf,
        OemQuotes = 0xde,
        OemSemicolon = 0xba,
        Oemtilde = 0xc0,
        P = 80,
        Pa1 = 0xfd,
        Packet = 0xe7,
        PageDown = 0x22,
        PageUp = 0x21,
        Pause = 0x13,
        Play = 250,
        Print = 0x2a,
        PrintScreen = 0x2c,
        Prior = 0x21,
        ProcessKey = 0xe5,
        Q = 0x51,
        R = 0x52,
        RButton = 2,
        RControlKey = 0xa3,
        Return = 13,
        Right = 0x27,
        RMenu = 0xa5,
        RShiftKey = 0xa1,
        RWin = 0x5c,
        S = 0x53,
        Scroll = 0x91,
        Select = 0x29,
        SelectMedia = 0xb5,
        Separator = 0x6c,
        Shift = 0x10000,
        ShiftKey = 0x10,
        Sleep = 0x5f,
        Snapshot = 0x2c,
        Space = 0x20,
        Subtract = 0x6d,
        T = 0x54,
        Tab = 9,
        U = 0x55,
        Up = 0x26,
        V = 0x56,
        VolumeDown = 0xae,
        VolumeMute = 0xad,
        VolumeUp = 0xaf,
        W = 0x57,
        X = 0x58,
        XButton1 = 5,
        XButton2 = 6,
        Y = 0x59,
        Z = 90,
        Zoom = 0xfb
    }
}
