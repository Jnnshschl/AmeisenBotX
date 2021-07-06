using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Hook.Structs
{
    /// <summary>
    /// General game information will be stored in this struct.
    ///
    /// There exists one instance in wow's memory that will be
    /// modified by asm code. This saves us a lot of calls to
    /// engine functions.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct GameInfo
    {
        public bool isOutdoors;
        public bool isTargetInLineOfSight;
    }
}