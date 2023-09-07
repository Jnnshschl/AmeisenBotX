namespace AmeisenBotX.Wow.Objects
{
    public interface IWowMount
    {
        int Index { get; set; }
        int MountId { get; set; }
        string Name { get; set; }
        int SpellId { get; set; }
    }
}