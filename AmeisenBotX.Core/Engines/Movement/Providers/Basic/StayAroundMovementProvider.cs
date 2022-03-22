using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using System;

namespace AmeisenBotX.Core.Engines.Movement.Providers.Basic
{
    public class StayAroundMovementProvider : IMovementProvider
    {
        public StayAroundMovementProvider(Func<(IWowUnit, float, float)> getUnit)
        {
            GetUnit = getUnit;
        }

        public Func<(IWowUnit, float, float)> GetUnit { get; }

        public bool Get(out Vector3 position, out MovementAction type)
        {
            (IWowUnit unit, float angle, float distance) = GetUnit();

            if (IWowUnit.IsValid(unit))
            {
                type = MovementAction.Move;
                position = BotMath.CalculatePositionAround(unit.Position, unit.Rotation, angle, distance);
                return true;
            }

            type = MovementAction.None;
            position = Vector3.Zero;
            return false;
        }
    }
}