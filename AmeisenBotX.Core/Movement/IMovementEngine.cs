using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement
{
    public interface IMovementEngine
    {
        List<Vector3> Path { get; }

        MovementEngineState State { get; }

        void Execute();

        void Reset();

        void SetState(MovementEngineState state, Vector3 position, float targetRotation = 0f);
    }
}