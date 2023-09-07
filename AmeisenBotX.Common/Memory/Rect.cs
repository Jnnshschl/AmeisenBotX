using System.Runtime.InteropServices;

namespace AmeisenBotX.Common.Memory
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left { get; set; }

        public int Top { get; set; }

        public int Right { get; set; }

        public int Bottom { get; set; }

        public static bool operator !=(Rect left, Rect right)
        {
            return !(left == right);
        }

        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof(Rect)
                       && ((Rect)obj).Left == Left
                       && ((Rect)obj).Top == Top
                       && ((Rect)obj).Right == Right
                       && ((Rect)obj).Bottom == Bottom;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return 17 + (Left * 23) + (Top * 23) + (Right * 23) + (Bottom * 23);
            }
        }

        public override string ToString()
        {
            return $"Left: {Left} Top: {Top} Right: {Right} Bottom: {Bottom}";
        }
    }
}