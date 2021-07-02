using AmeisenBotX.Common.Math;
using System.Runtime.InteropServices;

namespace AmeisenBotX.Wow.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public record RawCameraInfo
    {
        public uint VTable { get; set; }
        public uint Unk1 { get; set; }
        public Vector3 Pos { get; set; }
        public Matrix3x3 ViewMatrix { get; set; }
        public float Fov { get; set; }
        public float Unk2 { get; set; }
        public int Unk3 { get; set; }
        public float ZNearPlane { get; set; }
        public float ZFarPlane { get; set; }
        public float Aspect { get; set; }
    }
}