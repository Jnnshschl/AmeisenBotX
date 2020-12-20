using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Movement.Pathfinding.Objects
{
    class SearchAreaEnsamble
    {
        private List<SearchArea> Areas { get; } = new();
        private int CurrentSearchArea { get; set; }
        private Vector3 LastSearchPosition { get; set; } = Vector3.Zero;
        private bool AbortedPath { get; set; } = true;

        public SearchAreaEnsamble(List<List<Vector3>> searchAreas)
        {
            CurrentSearchArea = 0;
            foreach (List<Vector3> area in searchAreas)
            {
                Areas.Add(new SearchArea(area));
            }
        }

        public void NotifyDetour()
        {
            AbortedPath = true;
        }

        public bool HasAbortedPath()
        {
            return AbortedPath;
        }

        public Vector3 GetNextPosition(WowInterface wowInterface)
        {
            if (!AbortedPath || LastSearchPosition == Vector3.Zero)
            {
                LastSearchPosition = GetNextPositionInternal(wowInterface);
            }
            AbortedPath = false;
            return LastSearchPosition;
        }

        public bool IsPlayerNearSearchArea(WowInterface wowInterface)
        {
            return Areas[CurrentSearchArea].ContainsPosition(wowInterface.ObjectManager.Player.Position) 
                   || Areas[CurrentSearchArea].GetClosestVertexDistance(wowInterface.ObjectManager.Player.Position) <= 20.0;
        }

        private Vector3 GetNextPositionInternal(WowInterface wowInterface)
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
