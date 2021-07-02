namespace AmeisenBotX.Core.Engines.Character.Talents.Objects
{
    public class Talent
    {
        public Talent(int tab, int num, int rank)
        {
            Tab = tab;
            Num = num;
            Rank = rank;
        }

        public Talent(string name, int tab, int num, int rank, int maxRank)
        {
            Name = name;
            Tab = tab;
            Num = num;
            Rank = rank;
            MaxRank = maxRank;
        }

        public int MaxRank { get; set; }

        public string Name { get; set; }

        public int Num { get; set; }

        public int Rank { get; set; }

        public int Tab { get; set; }

        public override string ToString()
        {
            return $"[{Tab}][{Num}] {Name}: {Rank}/{MaxRank}";
        }
    }
}