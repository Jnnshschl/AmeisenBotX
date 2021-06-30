using AmeisenBotX.Core.Character.Talents.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.Character.Talents
{
    public class TalentManager
    {
        public TalentManager(AmeisenBotInterfaces bot)
        {
            Bot = bot;
        }

        public TalentTree TalentTree { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        public void SelectTalents(TalentTree wantedTalents, int talentPoints)
        {
            Dictionary<int, Dictionary<int, Talent>> talentTrees = TalentTree.AsDict();
            Dictionary<int, Dictionary<int, Talent>> wantedTalentTrees = wantedTalents.AsDict();

            List<(int, int, int)> talentsToSpend = new();

            // order the trees to skill the main tree first
            foreach (KeyValuePair<int, Dictionary<int, Talent>> kv in wantedTalentTrees.OrderByDescending(e => e.Value.Count))
            {
                if (CheckTalentTree(ref talentPoints, kv.Key, talentTrees[kv.Key], kv.Value, out List<(int, int, int)> newTalents))
                {
                    talentsToSpend.AddRange(newTalents);
                }
            }

            if (talentsToSpend.Any())
            {
                SpendTalents(talentsToSpend);
            }
        }

        public void Update()
        {
            TalentTree = new(Bot.Wow.LuaGetTalents());
        }

        private static bool CheckTalentTree(ref int talentPoints, int treeId, Dictionary<int, Talent> tree, Dictionary<int, Talent> wantedTree, out List<(int, int, int)> talentsToSpend)
        {
            talentsToSpend = new();

            if (talentPoints == 0)
            {
                return false;
            }

            bool result = false;

            Talent[] wantedTreeValues = wantedTree.Values.ToArray();

            for (int i = 0; i < wantedTreeValues.Length; ++i)
            {
                if (talentPoints == 0) { break; }

                Talent wantedTalent = wantedTreeValues[i];

                if (tree.ContainsKey(wantedTalent.Num))
                {
                    int wantedRank = Math.Min(wantedTalent.Rank, tree[wantedTalent.Num].MaxRank);

                    if (tree[wantedTalent.Num].Rank < wantedRank)
                    {
                        int amount = Math.Min(talentPoints, wantedRank - tree[wantedTalent.Num].Rank);

                        talentsToSpend.Add((treeId, wantedTalent.Num, amount));

                        talentPoints -= amount;
                        result = true;
                    }
                }
            }

            return result;
        }

        private void SpendTalents(List<(int, int, int)> talentsToSpend)
        {
            StringBuilder sb = new();

            for (int i = 0; i < talentsToSpend.Count; ++i)
            {
                sb.Append($"AddPreviewTalentPoints({talentsToSpend[i].Item1},{talentsToSpend[i].Item2},{talentsToSpend[i].Item3});");
            }

            sb.Append("LearnPreviewTalents();");

            Bot.Wow.LuaDoString(sb.ToString());
        }
    }
}