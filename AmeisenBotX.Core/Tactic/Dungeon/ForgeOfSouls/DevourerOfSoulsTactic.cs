using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Wow.Objects.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Tactic.Dungeon.ForgeOfSouls
{
    public class DevourerOfSoulsTactic : ITactic
    {
        public DevourerOfSoulsTactic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;

            Configureables = new()
            {
                { "isOffTank", false },
            };
        }

        public static Vector3 MidPosition { get; } = new Vector3(5662, 2507, 709);

        public Dictionary<string, dynamic> Configureables { get; private set; }

        public WowInterface WowInterface { get; }

        private static List<int> DevourerOfSoulsDisplayId { get; } = new List<int> { 30148, 30149, 30150 };

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            preventMovement = false;
            allowAttacking = true;

            WowUnit wowUnit = WowInterface.Objects.GetClosestWowUnitByDisplayId(WowInterface.Player.Position,DevourerOfSoulsDisplayId, false);

            if (wowUnit != null)
            {
                if (wowUnit.DisplayId == 30150)
                {
                    // make sure we avoid the lazer
                    // we only care about being on the reight side of him because the lazer spins clockwise
                    float angleDiff = BotMath.GetAngleDiff(wowUnit.Position, wowUnit.Rotation, WowInterface.Player.Position);

                    if (angleDiff < 0.5f)
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, BotMath.CalculatePositionAround(wowUnit.Position, wowUnit.Rotation, MathF.PI, isMelee ? 5.0f : 22.0f));

                        preventMovement = true;
                        allowAttacking = false;
                        return true;
                    }
                }

                if (role == WowRole.Tank)
                {
                    Vector3 modifiedCenterPosition = BotUtils.MoveAhead(MidPosition, BotMath.GetFacingAngle(WowInterface.Objects.MeanGroupPosition, MidPosition), 8.0f);
                    float distanceToMid = WowInterface.Player.Position.GetDistance(modifiedCenterPosition);

                    if (wowUnit.TargetGuid == WowInterface.Player.Guid)
                    {
                        if (distanceToMid > 5.0f && WowInterface.Player.Position.GetDistance(wowUnit.Position) < 3.5)
                        {
                            // move the boss to mid
                            WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, modifiedCenterPosition);

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