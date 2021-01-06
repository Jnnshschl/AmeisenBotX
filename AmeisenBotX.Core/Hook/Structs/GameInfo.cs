using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Hook.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GameInfo
    {
        public bool isOutdoors;
        public bool isTargetInLineOfSight;
    }
}