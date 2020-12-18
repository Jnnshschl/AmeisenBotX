using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Movement.Pathfinding.Objects
{
    class SearchAreaEnsemble
    {
        private List<SearchArea> Areas { get; } = new();
        private int CurrentSearchArea { get; set; }
        public SearchAreaEnsemble(List<List<Vector3>> searchAreas)
        {
            CurrentSearchArea = 0;
            foreach (List<Vector3> area in searchAreas)
            {
                Areas.Add(new SearchArea(area));
            }
        }

        public Vector3 GetNextPosition(WowInterface wowInterface)
        {
            Vector3 currentPosition = wowInterface.ObjectManager.Player.Position;
            if (Areas[CurrentSearchArea].ContainsPosition(currentPosition))
            {
                Vector3 position = Areas[CurrentSearchArea].GetNextSearchPosition();
                if (Areas[CurrentSearchArea].IsAtTheBeginning())
                {
                    CurrentSearchArea = (CurrentSearchArea + 1) % Areas.Count;
                }
                return position;
            }

            return Areas[CurrentSearchArea].GetClosestEntry(wowInterface);
        }

    }
}
