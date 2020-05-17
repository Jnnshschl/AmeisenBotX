using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public interface IMovementEngine
    {
        MovementEngineState State { get; }

        void Execute();

        void Reset();

        List<Vector3> Path { get; }

        void SetState(MovementEngineState state, Vector3 position, float targetRotation = 0f);
    }
}