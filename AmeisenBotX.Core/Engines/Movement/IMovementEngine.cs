using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Movement
{
    public interface IMovementEngine
    {
        /// <summary>
        /// Returns the current speed in meters per second.
        /// </summary>
        float CurrentSpeed { get; }

        /// <summary>
        /// Get the curren loaded path.
        /// </summary>
        IEnumerable<Vector3> Path { get; }

        /// <summary>
        /// Get the current blacklisted places.
        /// </summary>
        IEnumerable<(Vector3 position, float radius)> PlacesToAvoid { get; }

        /// <summary>
        /// Get the current movement engine state.
        /// </summary>
        MovementAction Status { get; }

        /// <summary>
        /// Add a place to the blacklist. Used to avoid AOE effects.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="timeSpan">How long should the place be blacklisted</param>
        void AvoidPlace(Vector3 position, float radius, TimeSpan timeSpan);

        /// <summary>
        /// Poll this on a regular basis to execute the movement.
        /// </summary>
        void Execute();

        /// <summary>
        /// Determine wheter a position can be reached by the player.
        /// </summary>
        /// <param name="position">Target position</param>
        /// <param name="path">The resulting path if the target position can be reached</param>
        /// <param name="maxDistance">Max distance to the target position</param>
        /// <returns>True if it can be reached, false if not</returns>
        bool TryGetPath(Vector3 position, out IEnumerable<Vector3> path, float maxDistance = 1.0f);

        /// <summary>
        /// Prevent movement for a specified time.
        /// </summary>
        /// <param name="timeSpan">How long should movement be prevented</param>
        void PreventMovement(TimeSpan timeSpan);

        /// <summary>
        /// Drop the current target position and path.
        /// </summary>
        void Reset();

        /// <summary>
        /// Set a new target position.
        /// </summary>
        /// <param name="state">How should the bot move</param>
        /// <param name="position">Where should the bot move</param>
        /// <param name="rotation">target rotation</param>
        /// <returns>True if instruction was set successful, false if not</returns>
        bool SetMovementAction(MovementAction state, Vector3 position, float rotation = 0);

        /// <summary>
        /// Stop the bots current movement.
        /// </summary>
        void StopMovement();
    }
}