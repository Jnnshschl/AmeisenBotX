using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;

namespace AmeisenBotX.Core.Engines.Movement
{
    public interface IMovementProvider
    {
        bool Get(out Vector3 position, out MovementAction type);
    }
}