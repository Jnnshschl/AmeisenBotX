namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowDynobject : WowObject
    {
        public ulong CasterGuid { get; set; }
        public float Facing { get; set; }
        public float Radius { get; set; }
        public int SpellId { get; set; }
    }
}