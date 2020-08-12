using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public interface IMovementEngine
    {
        bool IsAtTargetPosition { get; }

        bool IsGhost { get; set; }

        PathfindingStatus PathfindingStatus { get; }

        MovementAction MovementAction { get; }

        List<Vector3> Path { get; }

        void Execute();

        bool HasCompletePathToPosition(Vector3 position, double maxDistance);

        void Reset();

        void SetMovementAction(MovementAction state, Vector3 position, float targetRotation = 0f, bool disableShortcuts = false);

        void StopMovement();
    }
}