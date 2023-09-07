using System;

namespace AmeisenBotX.Wow.Objects.Flags
{
    [Flags]
    public enum WowMovementDirectionFlag
    {
        None = 0x0,                    // HEX: 0x0000 0000 - DEC: 0
        RMouse = 0x1,                  // HEX: 0x0000 0001 - DEC: 1
        LMouse = 0x2,                  // HEX: 0x0000 0002 - DEC: 2
        Forward = 0x10,                // HEX: 0x0000 0010 - DEC: 16
        Backwards = 0x20,              // HEX: 0x0000 0020 - DEC: 32
        StrafeLeft = 0x40,             // HEX: 0x0000 0040 - DEC: 64
        StrafeRight = 0x80,            // HEX: 0x0000 0080 - DEC: 128
        TurnLeft = 0x100,              // HEX: 0x0000 0100 - DEC: 256
        TurnRight = 0x200,             // HEX: 0x0000 0200 - DEC: 512
        PitchUp = 0x400,               // HEX: 0x0000 0400 - DEC: 1024; Note: flying/swimming
        PitchDown = 0x800,             // HEX: 0x0000 0800 - DEC: 2048; Note: flying/swimming
        AutoRun = 0x1000,              // HEX: 0x0000 1000 - DEC: 4096
        JumpAscend = 0x2000,           // HEX: 0x0000 2000 - DEC: 8192; Note: flying/swimming
        Descend = 0x4000,              // HEX: 0x0000 4000 - DEC: 16384; Note: flying/swimming

        // ???? HEX: 0x0000 8000 - DEC: 32768
        ForwardBackMovement = 0x10000, // HEX: 0x0001 0000 - DEC: 65536

        StrafeMovement = 0x20000,      // HEX: 0x0002 0000 - DEC: 131072
        TurnMovement = 0x40000,        // HEX: 0x0004 0000 - DEC: 262144

        // ???? HEX: 0x0008 0000 - DEC: 524288 ???? HEX: 0x0010 0000 - DEC: 1048576
        IsCTMing = 0x200000,          // HEX: 0x0020 0000 - DEC: 2097152

        ClickToMove = 0x400000,        // HEX: 0x0040 0000 - DEC: 4194304; Note: Only turns the CTM flag on or off.
    }
}