namespace AmeisenBotX.Core.Managers.Character.Talents.Objects
{
    public interface ITalent
    {
        int MaxRank { get; set; }
        string Name { get; set; }
        int Num { get; set; }
        int Rank { get; set; }
        int Tab { get; set; }

        string ToString();
    }
}