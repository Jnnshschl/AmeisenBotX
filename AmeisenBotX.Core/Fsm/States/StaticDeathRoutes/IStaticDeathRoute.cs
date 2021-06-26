using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Fsm.States.StaticDeathRoutes
{
    interface IStaticDeathRoute
    {
        bool IsUseable(WowMapId mapId, Vector3 start, Vector3 end);

        void Init();

        Vector3 GetNextPoint(Vector3 playerPosition);
    }
}
