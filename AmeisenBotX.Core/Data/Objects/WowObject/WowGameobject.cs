using System.Collections.Specialized;

namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowGameobject : WowObject
    {
        public int DisplayId { get; set; }

        public BitVector32 DynamicFlags { get; set; }

        public float Facing { get; set; }

        public int Faction { get; set; }

        public BitVector32 Flags { get; set; }

        public WowGameobjectType GameobjectType { get; set; }

        public int Level { get; set; }

        public float Rotation { get; set; }

        public int State { get; set; }
    }
}