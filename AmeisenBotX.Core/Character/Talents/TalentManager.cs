using AmeisenBotX.Core.Character.Talents.Objects;
using System;
using System.Collections.Generic;

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
            talentPoints = CheckTalentTree(1, talentPoints, TalentTree.Tree1, wantedTalents.Tree1);

            if (talentPoints == 0) { return; }
            talentPoints = CheckTalentTree(2, talentPoints, TalentTree.Tree2, wantedTalents.Tree2);

            if (talentPoints == 0) { return; }
            CheckTalentTree(3, talentPoints, TalentTree.Tree3, wantedTalents.Tree3);
        }

        public void Update()
        {
            TalentTree = new TalentTree(WowInterface.HookManager.GetTalents());
        }

        private int CheckTalentTree(int id, int talentPoints, Dictionary<int, Talent> tree, Dictionary<int, Talent> wantedTree)
        {
            bool selectedTab = false;
            foreach (Talent wantedTalent in wantedTree.Values)
            {
                if (tree.ContainsKey(wantedTalent.Num))
                {
                    int wantedRank = Math.Min(wantedTalent.Rank, tree[wantedTalent.Num].MaxRank);

                    if (tree[wantedTalent.Num].Rank < wantedRank)
                    {
                        if (!selectedTab)
                        {
                            SelectTalentTab(id);
                            selectedTab = true;
                        }

                        for (int i = 0; i < wantedRank - tree[wantedTalent.Num].Rank; ++i)
                        {
                            SpendTalent(wantedTalent.Num);
                            --talentPoints;
                        }
                    }
                }
            }

            return talentPoints;
        }

        private void SelectTalentTab(int id)
        {
            WowInterface.HookManager.ClickUiElement($"PlayerTalentFrameTab{id}");
        }

        private void SpendTalent(int id)
        {
            WowInterface.HookManager.ClickUiElement($"PlayerTalentFrameTalent{id}");
        }
    }
}