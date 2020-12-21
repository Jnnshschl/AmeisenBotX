using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Tactic.Dungeon.ForgeOfSouls
{
    public class DevourerOfSoulsTactic : ITactic
    {
        private static List<int> DevourerOfSoulsDisplayId { get; } = new List<int> { 30148, 30149, 30150 };

        public static Vector3 MidPosition { get; } = new Vector3(5662, 2507, 709);

        public bool ExecuteTactic(CombatClassRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            preventMovement = false;
            allowAttacking = true;

            WowUnit wowUnit = WowInterface.I.ObjectManager.GetClosestWowUnitByDisplayId(DevourerOfSoulsDisplayId, false);

            if (wowUnit != null)
            {
                if (wowUnit.DisplayId == 30150)
                {
                    // make sure we avoid the lazer
                    // we only care about being on the reight side of him because the lazer spins clockwise
                    float angleDiff = BotMath.GetAngleDiff(wowUnit.Position, wowUnit.Rotation, WowInterface.I.ObjectManager.Player.Position);

                    if (angleDiff < 0.5f)
                    {
                        WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Moving, BotMath.CalculatePositionAround(wowUnit.Position, wowUnit.Rotation, MathF.PI, isMelee ? 5.0f : 22.0f));

                        preventMovement = true;
                        allowAttacking = false;
                        return true;
                    }
                }

                if (role == CombatClassRole.Tank)
                {
                    Vector3 modifiedCenterPosition = BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(WowInterface.I.ObjectManager.MeanGroupPosition, MidPosition), 8.0f);
                    float distanceToMid = WowInterface.I.ObjectManager.Player.Position.GetDistance(modifiedCenterPosition);

                    if (wowUnit.TargetGuid == WowInterface.I.ObjectManager.PlayerGuid)
                    {
                        if (distanceToMid > 5.0f && WowInterface.I.ObjectManager.Player.Position.GetDistance(wowUnit.Position) < 3.5)
                        {
                            // move the boss to mid
                            WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Moving, modifiedCenterPosition);

                            preventMovement = true;
                            allowAttacking = false;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}

