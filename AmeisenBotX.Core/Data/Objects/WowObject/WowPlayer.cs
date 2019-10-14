namespace AmeisenBotX.Core.Data.Objects.WowObject
{
    public class WowPlayer : WowUnit
    {
        public WowClass Class { get; set; }

        public int Exp { get; set; }

        public int MaxExp { get; set; }

        public WowRace Race { get; set; }
    }
}