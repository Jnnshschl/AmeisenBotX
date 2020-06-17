using AmeisenBotX.Core.Character.Talents.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmeisenBotX.Core.Character.Talents
{
    public class TalentManager
    {
        public TalentManager(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
        }

        public TalentTree TalentTree { get; set; }

        private WowInterface WowInterface { get; }

        public void SelectTalents(TalentTree wantedTalents, int talentPoints)
        {
            Dictionary<int, Dictionary<int, Talent>> talentTrees = TalentTree.AsDict();
            Dictionary<int, Dictionary<int, Talent>> wantedTalentTrees = wantedTalents.AsDict();

            List<(int, int, int)> talentsToSpend = new List<(int, int, int)>();

            // order the trees to skill the main tree first
            foreach (KeyValuePair<int, Dictionary<int, Talent>> kv in wantedTalentTrees.OrderByDescending(e => e.Value.Count()))
            {
                if (CheckTalentTree(ref talentPoints, kv.Key, talentTrees[kv.Key], kv.Value, out List<(int, int, int)> newTalents))
                {
                    talentsToSpend.AddRange(newTalents);
                }
            }

            if (talentsToSpend.Count > 0)
            {
                SpendTalents(talentsToSpend);
            }
        }

        public void Update()
        {
            TalentTree = new TalentTree(WowInterface.HookManager.GetTalents());
        }

        private bool CheckTalentTree(ref int talentPoints, int treeId, Dictionary<int, Talent> tree, Dictionary<int, Talent> wantedTree, out List<(int, int, int)> talentsToSpend)
        {
            talentsToSpend = new List<(int, int, int)>();

            if (talentPoints == 0)
            {
                return false;
            }

            bool result = false;

            foreach (Talent wantedTalent in wantedTree.Values)
            {
                if (talentPoints == 0) { break; }

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
            StringBuilder sb = new StringBuilder();

            foreach ((int, int, int) talent in talentsToSpend)
            {
                sb.Append($"AddPreviewTalentPoints({talent.Item1},{talent.Item2},{talent.Item3});");
            }

            sb.Append("LearnPreviewTalents();");

            WowInterface.HookManager.LuaDoString(sb.ToString());
        }
    }
}