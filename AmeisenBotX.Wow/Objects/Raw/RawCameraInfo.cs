using AmeisenBotX.Common.Math;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawCameraInfo
    {
        public uint VTable;
        public uint Unk1;
        public Vector3 Pos;
        public Matrix3x3 ViewMatrix;
        public float Fov;
        public float Unk2;
        public int Unk3;
        public float ZNearPlane;
        public float ZFarPlane;
        public float Aspect;
    }
}