using AmeisenBotX.Common.Math;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Movement.Pathfinding.Objects
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

        private List<SearchArea> Areas { get; } = [];

        private int CurrentSearchArea { get; set; }

        private Vector3 LastSearchPosition { get; set; } = Vector3.Zero;

        public Vector3 GetNextPosition(AmeisenBotInterfaces bot)
        {
            if (!AbortedPath || LastSearchPosition == Vector3.Zero)
            {
                LastSearchPosition = GetNextPositionInternal(bot);
            }

            AbortedPath = false;
            return LastSearchPosition;
        }

        public bool HasAbortedPath()
        {
            return AbortedPath;
        }

        public bool IsPlayerNearSearchArea(AmeisenBotInterfaces bot)
        {
            return Areas[CurrentSearchArea].ContainsPosition(bot.Objects.Player.Position)
                   || Areas[CurrentSearchArea].GetClosestVertexDistance(bot.Objects.Player.Position) <= 20.0;
        }

        public void NotifyDetour()
        {
            AbortedPath = true;
        }

        private Vector3 GetNextPositionInternal(AmeisenBotInterfaces bot)
        {
            Vector3 currentPosition = bot.Objects.Player.Position;

            if (Areas[CurrentSearchArea].ContainsPosition(currentPosition))
            {
                Vector3 position = Areas[CurrentSearchArea].GetNextSearchPosition();

                if (Areas[CurrentSearchArea].IsAtTheBeginning())
                {
                    CurrentSearchArea = (CurrentSearchArea + 1) % Areas.Count;
                }

                return position;
            }

            return Areas[CurrentSearchArea].GetClosestEntry(bot);
        }
    }
}