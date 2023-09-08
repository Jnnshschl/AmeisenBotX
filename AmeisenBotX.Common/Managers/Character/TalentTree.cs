﻿using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character.Talents.Objects
{
    public class TalentTree : ITalentTree
    {
        public TalentTree(string talentString)
        {
            Tree1 = new();
            Tree2 = new();
            Tree3 = new();

            string[] talentSplits = talentString.Split('|');

            foreach (string talent in talentSplits)
            {
                if (talent.Length < 4) { continue; }

                string[] items = talent.Split(';');

                if (items.Length < 5) { continue; }

                Talent t = new(items[0], int.Parse(items[1]), int.Parse(items[2]), int.Parse(items[3]), int.Parse(items[4]));

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
            return new()
            {
                { 1, Tree1 },
                { 2, Tree2 },
                { 3, Tree3 },
            };
        }
    }
}