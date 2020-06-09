using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public interface IMovementEngine
    {
        MovementAction MovementAction { get; }

        List<Vector3> Path { get; }

        void Execute();

        void Reset();

        void SetMovementAction(MovementAction state, Vector3 position, float targetRotation = 0f);
    }
}