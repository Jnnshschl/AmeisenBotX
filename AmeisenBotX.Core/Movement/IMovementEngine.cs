using AmeisenBotX.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public interface IMovementEngine
    {
        Queue<Vector3> CurrentPath { get; }

        /// <summary>
        /// Load a Path (List<Vector3>) to process the movement on.
        /// You may obtain this Path from any IPathfindingHandler.
        /// </summary>
        /// <param name="path">The Path to process</param>
        void LoadPath(List<Vector3> path);

        /// <summary>
        /// Clears the current Path and resets the IMovementEngine to
        /// its default state.
        /// </summary>
        void Reset();

        /// <summary>
        /// (OPTIONAL) This Method may be used to optimize the given
        /// Path but it don't has to. Read the specialized
        /// documentation of your class to find out what it does.
        ///
        /// Example of optimizations:
        /// - Make the Path Smooth
        /// - Optimize sharp corners of the Path
        /// </summary>
        void PostProcessPath();

        /// <summary>
        /// Get the Position (Vector3) that you need to got to based
        /// on your current Position.
        /// </summary>
        /// <param name="currentPosition">Your current Position as a Vector3</param>
        /// <param name="positionToGoTo">The next step to got to as a Vector3</param>
        /// <param name="needToJump">Wether we need to jump or not</param>
        /// <returns>Wether there is a next step or not</returns>
        bool GetNextStep(Vector3 currentPosition, float currentRotation, out Vector3 positionToGoTo, out bool needToJump);
    }
}