using AmeisenBotX.Core.Managers.Character.Talents.Objects;
using AmeisenBotX.Wow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.Managers.Character.Talents
{
    public class TalentManager
    {
        public TalentManager(IWowInterface wowInterface)
        {
            Wow = wowInterface;
        }

        public TalentTree TalentTree { get; set; }

        private IWowInterface Wow { get; }

        public void SelectTalents(TalentTree wantedTalents, int talentPoints)
        {
            Dictionary<int, Dictionary<int, Talent>> talentTrees = TalentTree.AsDict();
            Dictionary<int, Dictionary<int, Talent>> wantedTalentTrees = wantedTalents.AsDict();

            List<(int, int, int)> talentsToSpend = [];

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
            TalentTree = new(Wow.GetTalents());
        }

        private static bool CheckTalentTree(ref int talentPoints, int treeId, Dictionary<int, Talent> tree, Dictionary<int, Talent> wantedTree, out List<(int, int, int)> talentsToSpend)
        {
            talentsToSpend = [];

            if (talentPoints == 0)
            {
                return false;
            }

            bool result = false;

            Talent[] wantedTreeValues = wantedTree.Values.ToArray();

            foreach (Talent talent in wantedTreeValues)
            {
                if (talentPoints == 0) { break; }

                Talent wantedTalent = talent;

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

            Wow.LuaDoString(sb.ToString());
        }
    }
}