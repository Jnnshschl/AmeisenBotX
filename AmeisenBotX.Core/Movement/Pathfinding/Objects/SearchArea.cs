﻿using AmeisenBotX.Common.Math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Movement.Pathfinding.Objects
{
    internal class SearchArea
    {
        public SearchArea(List<Vector3> searchArea)
        {
            Area = searchArea;
            CurrentSearchPathIndex = 0;
            CalculateSearchPath();
        }

        private List<Vector3> Area { get; }

        private int CurrentSearchPathIndex { get; set; }

        private List<Vector3> SearchPath { get; set; }

        private float VisibilityRadius { get; } = 30.0f;

        public bool ContainsPosition(Vector3 position)
        {
            if (Area.Count <= 1)
            {
                return false;
            }

            // Ray Casting algorithm
            float x = position.X;
            float y = position.Y;
            bool inside = false;

            for (int i = 0, j = Area.Count - 1; i < Area.Count; j = i++)
            {
                float xi = Area[i].X;
                float yi = Area[i].Y;
                float xj = Area[j].X;
                float yj = Area[j].Y;

                if (((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public Vector3 GetClosestEntry(AmeisenBotInterfaces bot)
        {
            if (Area.Count == 1)
            {
                return Area[0];
            }

            Vector3 currentPosition = bot.Objects.Player.Position;

            // This is not optimal but fairly simple
            // We ask for the path for every vertex.
            // It could be optimized by following the edges up or down and stop once the distance increased in both directions.
            // We dont ask for the Distance2D because we want to know the movement path length

            List<double> distances = new();

            foreach (Vector3 vertex in Area)
            {
                double totalDistance = 0.0;
                IEnumerable<Vector3> path = bot.PathfindingHandler.GetPath((int)bot.Objects.MapId, currentPosition, vertex);

                if (path != null)
                {
                    Vector3 lastPosition = currentPosition;

                    foreach (Vector3 pathPosition in path)
                    {
                        totalDistance += pathPosition.GetDistance(lastPosition);
                        lastPosition = pathPosition;
                    }
                }

                distances.Add(totalDistance);
            }

            int minimumIndex = distances.IndexOf(distances.Min());
            Vector3 entryPosition = Area[minimumIndex];

            // The ContainsPoint function is sensible towards edges, therefore we will wiggle us into the polygon
            if (!ContainsPosition(entryPosition))
            {
                {
                    Vector3 newEntryPosition = new(entryPosition);
                    newEntryPosition.X += VisibilityRadius / 2;
                    newEntryPosition.Y += VisibilityRadius / 2;

                    if (ContainsPosition(newEntryPosition))
                    {
                        return newEntryPosition;
                    }
                }

                {
                    Vector3 newEntryPosition = new(entryPosition);
                    newEntryPosition.X -= VisibilityRadius / 2;
                    newEntryPosition.Y -= VisibilityRadius / 2;

                    if (ContainsPosition(newEntryPosition))
                    {
                        return newEntryPosition;
                    }
                }

                {
                    Vector3 newEntryPosition = new(entryPosition);
                    newEntryPosition.X += VisibilityRadius / 2;
                    newEntryPosition.Y -= VisibilityRadius / 2;

                    if (ContainsPosition(newEntryPosition))
                    {
                        return newEntryPosition;
                    }
                }

                {
                    Vector3 newEntryPosition = new(entryPosition);
                    newEntryPosition.X -= VisibilityRadius / 2;
                    newEntryPosition.Y += VisibilityRadius / 2;

                    if (ContainsPosition(newEntryPosition))
                    {
                        return newEntryPosition;
                    }
                }
            }

            return entryPosition;
        }

        public float GetClosestVertexDistance(Vector3 position)
        {
            return Area.Select(pos => pos.GetDistance(position)).Min();
        }

        public Vector3 GetNextSearchPosition()
        {
            if (Area.Count == 1 || SearchPath.Count == 0)
            {
                return Area[0];
            }

            Vector3 position = SearchPath[CurrentSearchPathIndex];
            CurrentSearchPathIndex = (CurrentSearchPathIndex + 1) % SearchPath.Count;
            return position;
        }

        public bool IsAtTheBeginning()
        {
            return CurrentSearchPathIndex == 0;
        }

        private void CalculateSearchPath()
        {
            if (Area.Count <= 1)
            {
                SearchPath = Area;
                return;
            }

            // This is not optimal but should be fast
            // We raster the polygon with points apart 2x VisibilityRadius
            // We then remove all points that are not in the polygon
            // Finally we move to one point after another

            // First find top, right, left, right
            float top = Area[0].Y;
            float right = Area[0].X;
            float left = Area[0].X;
            float bottom = Area[0].Y;
            float maxZ = Area[0].Y;

            foreach (Vector3 vertex in Area)
            {
                top = MathF.Max(vertex.Y, top);
                right = MathF.Max(vertex.X, right);
                left = MathF.Min(vertex.X, left);
                bottom = MathF.Min(vertex.Y, bottom);
                maxZ = MathF.Max(vertex.Z, maxZ);
            }

            // Raster the rectangle and add fitting points
            int stepsTopToBottom = (int)MathF.Ceiling(MathF.Abs(top - bottom) / VisibilityRadius);
            int stepsLeftToRight = (int)MathF.Ceiling(MathF.Abs(left - right) / VisibilityRadius);

            float leftStart = left - VisibilityRadius / 2;
            float topStart = top + VisibilityRadius / 2;

            bool directionToggle = false;
            List<Vector3> newSearchPath = new();

            for (int y = 0; y < stepsTopToBottom - 1; ++y)
            {
                topStart += VisibilityRadius;

                for (int x = 0; x < stepsLeftToRight - 1; ++x)
                {
                    if (directionToggle)
                    {
                        leftStart -= VisibilityRadius;
                    }
                    else
                    {
                        leftStart += VisibilityRadius;
                    }

                    Vector3 newVertex = new(leftStart, topStart, maxZ);

                    if (ContainsPosition(newVertex))
                    {
                        newSearchPath.Add(newVertex);
                    }
                }

                directionToggle = !directionToggle;
            }

            SearchPath = newSearchPath;
        }
    }
}