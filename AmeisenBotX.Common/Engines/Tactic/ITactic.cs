using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Tactic
{
    public interface ITactic
    {
        Vector3 Area { get; }

        float AreaRadius { get; }

        Dictionary<string, dynamic> Configurables { get; }

        WowMapId MapId { get; }

        bool ExecuteTactic(WowRole role, bool isMelee, out bool handlesMovement, out bool allowAttacking);

        bool IsInArea(Vector3 position)
        {
            return position.GetDistance(Area) < AreaRadius;
        }
    }
}