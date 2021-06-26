using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Fsm.States.StaticDeathRoutes
{
    public interface IStaticDeathRoute
    {
        Vector3 GetNextPoint(Vector3 playerPosition);

        void Init(Vector3 playerPosition);

        bool IsUseable(WowMapId mapId, Vector3 start, Vector3 end);
    }
}