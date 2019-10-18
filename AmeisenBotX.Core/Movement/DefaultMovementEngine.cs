using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Pathfinding;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public class DefaultMovementEngine : IMovementEngine
    {
        public DefaultMovementEngine(MovementSettings settings = null)
        {
            if (settings == null)
            {
                settings = new MovementSettings();
            }

            Settings = settings;
        }

        public Queue<Vector3> CurrentPath { get; private set; }
        
        public Vector3 LastWaypoint { get; private set; }

        public MovementSettings Settings { get; private set; }

        public bool GetNextStep(Vector3 currentPosition, out Vector3 positionToGoTo)
        {
            if (CurrentPath == null)
            {
                throw new ArgumentNullException("CurrentPath", "You need to set a Path using LoadPath(...);");
            }

            Vector3 currentWaypoint = CurrentPath.Peek();
            if (currentPosition.GetDistance(currentWaypoint) <= Settings.WaypointCheckThreshold)
            {
                LastWaypoint = CurrentPath.Dequeue();
            }

            if (CurrentPath.Count > 0)
            {
                positionToGoTo = CurrentPath.Peek();
                return true;
            }

            Reset();
            positionToGoTo = default;
            return false;
        }

        public void LoadPath(List<Vector3> path)
        {
            CurrentPath = new Queue<Vector3>();
            foreach (Vector3 v in path)
            {
                CurrentPath.Enqueue(v);
            }
        }

        public void PostProcessPath()
        {
            // wont do anything here
        }

        public void Reset()
        {
            CurrentPath = null;
            LastWaypoint = default;
        }
    }
}
