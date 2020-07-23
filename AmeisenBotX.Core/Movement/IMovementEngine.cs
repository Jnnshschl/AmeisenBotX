using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public interface IMovementEngine
    {
        bool IsAtTargetPosition { get; }

        MovementAction MovementAction { get; }

        List<Vector3> Path { get; }

        void Execute();

        void Reset();

        void SetMovementAction(MovementAction state, Vector3 position, float targetRotation = 0f, double minDistanceToMove = 1.5);

        void StopMovement();

        bool HasCompletePathToPosition(Vector3 position, double maxDistance);
    }
}