using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public interface IMovementEngine
    {
        bool IsMoving { get; }

        IEnumerable<Vector3> Path { get; }

        IEnumerable<(Vector3 position, float radius)> PlacesToAvoid { get; }

        MovementAction Status { get; }

        void AvoidPlace(Vector3 position, float radius, TimeSpan timeSpan);

        void Execute();

        bool IsPositionReachable(Vector3 position, out IEnumerable<Vector3> path, float maxDistance = 1.0f);

        void PreventMovement(TimeSpan timeSpan);

        void Reset();

        bool SetMovementAction(MovementAction state, Vector3 position, float rotation = 0);

        void StopMovement();
    }
}