using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;

namespace AmeisenBotX.Core.Logic.Routines
{
    public static class CommonRoutines
    {
        public static bool MoveToTarget(AmeisenBotInterfaces bot, Vector3 position, float range, MovementAction action = MovementAction.Move)
        {
            if (!bot.Objects.IsTargetInLineOfSight || bot.Player.DistanceTo(position) > range)
            {
                bot.Movement.SetMovementAction(action, position);
                return true;
            }

            return false;
        }
    }
}