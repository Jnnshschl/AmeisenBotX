using System.Runtime.InteropServices;

namespace AmeisenBotX.Memory.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int Left { get; set; }

        public int Right { get; set; }

        public int Top { get; set; }

        public int Bottom { get; set; }

        public override bool Equals(object obj)
        {
            return obj != null
                && obj.GetType() == typeof(Margins)
                && ((Margins)obj).Left == Left
                && ((Margins)obj).Right == Right
                && ((Margins)obj).Top == Top
                && ((Margins)obj).Bottom == Bottom;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Left * 23 + Right * 23 + Top * 23 + Bottom * 23;
            }
        }

        public static bool operator ==(Margins left, Margins right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Margins left, Margins right)
        {
            return !(left == right);
        }
    }
}