using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Talents.Objects
{
    public class TalentTree
    {
        public TalentTree(string talentString)
        {
            Tree1 = new Dictionary<int, Talent>();
            Tree2 = new Dictionary<int, Talent>();
            Tree3 = new Dictionary<int, Talent>();

            string[] talentSplits = talentString.Split('|');

            for (int i = 0; i < talentSplits.Length; ++i)
            {
                string talent = talentSplits[i];

                if (talent.Length < 4) { continue; }

                string[] items = talent.Split(';');

                if (items.Length < 5) { continue; }

                Talent t = new Talent(items[0], int.Parse(items[1]), int.Parse(items[2]), int.Parse(items[3]), int.Parse(items[4]));

                if (items[1].Equals("1", StringComparison.OrdinalIgnoreCase))
                {
                    Tree1.Add(int.Parse(items[2]), t);
                }
                else if (items[1].Equals("2", StringComparison.OrdinalIgnoreCase))
                {
                    Tree2.Add(int.Parse(items[2]), t);
                }
                else if (items[1].Equals("3", StringComparison.OrdinalIgnoreCase))
                {
                    Tree3.Add(int.Parse(items[2]), t);
                }
            }
        }

        public TalentTree()
        {
        }

        public Dictionary<int, Talent> Tree1 { get; set; }

        public Dictionary<int, Talent> Tree2 { get; set; }

        public Dictionary<int, Talent> Tree3 { get; set; }

        public Dictionary<int, Dictionary<int, Talent>> AsDict()
        {
            return new Dictionary<int, Dictionary<int, Talent>>()
            {
                { 1, Tree1 },
                { 2, Tree2 },
                { 3, Tree3 },
            };
        }
    }
}