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
        }

        public Queue<Vector3> CurrentPath { get; private set; }

        public bool GetNextStep(Vector3 currentPosition, out Vector3 positionToGoTo)
        {
            if (CurrentPath == null)
            {
                throw new ArgumentNullException("CurrentPath", "You need to set a Path using LoadPath(...);");
            }

            positionToGoTo = new Vector3(0, 0, 0);
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

        }

        public void Reset()
        {
            CurrentPath = null;
        }
    }
}
