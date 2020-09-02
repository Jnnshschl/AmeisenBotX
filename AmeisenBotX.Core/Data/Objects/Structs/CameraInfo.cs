using System.Numerics;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Core.Data.Objects.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CameraInfo
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