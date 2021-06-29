using AmeisenBotX.Common.Math;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement.Pathfinding.Objects
{
    internal class SearchAreaEnsamble
    {
        public SearchAreaEnsamble(List<List<Vector3>> searchAreas)
        {
            CurrentSearchArea = 0;
            foreach (List<Vector3> area in searchAreas)
            {
                Areas.Add(new(area));
            }
        }

        private bool AbortedPath { get; set; } = true;

        private List<SearchArea> Areas { get; } = new();

        private int CurrentSearchArea { get; set; }

        private Vector3 LastSearchPosition { get; set; } = Vector3.Zero;

        public Vector3 GetNextPosition(WowInterface wowInterface)
        {
            if (!AbortedPath || LastSearchPosition == Vector3.Zero)
            {
                LastSearchPosition = GetNextPositionInternal(wowInterface);
            }

            AbortedPath = false;
            return LastSearchPosition;
        }

        public bool HasAbortedPath()
        {
            return AbortedPath;
        }

        public bool IsPlayerNearSearchArea(WowInterface wowInterface)
        {
            return Areas[CurrentSearchArea].ContainsPosition(wowInterface.Objects.Player.Position)
                   || Areas[CurrentSearchArea].GetClosestVertexDistance(wowInterface.Objects.Player.Position) <= 20.0;
        }

        public void NotifyDetour()
        {
            AbortedPath = true;
        }

        private Vector3 GetNextPositionInternal(WowInterface wowInterface)
        {
            Vector3 currentPosition = wowInterface.Objects.Player.Position;

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