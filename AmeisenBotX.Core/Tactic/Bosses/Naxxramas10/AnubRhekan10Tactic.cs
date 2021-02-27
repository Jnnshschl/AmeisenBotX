using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Tactic.Bosses.Naxxramas10
{
    public class AnubRhekan10Tactic : ITactic
    {
        public AnubRhekan10Tactic(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            TankingPathQueue = new Queue<Vector3>();

            Configureables = new Dictionary<string, dynamic>()
            {
                { "isOffTank", false },
            };
        }

        public Dictionary<string, dynamic> Configureables { get; private set; }

        public DateTime LocustSwarmActivated { get; private set; }

        private static List<int> AddsDisplayIds { get; } = new List<int> { 14698, 27943 };

        private static List<int> AnubRhekanDisplayId { get; } = new List<int> { 15931 };

        private Vector3 ImpaleDodgePos { get; set; }

        private bool LocustSwarmActive => (LocustSwarmActivated + TimeSpan.FromSeconds(20)) > DateTime.UtcNow;

        private bool MeleeDpsIsMovingToMid { get; set; } = false;

        private Vector3 MiddleSpot { get; } = new Vector3(3274, -3476, 287);

        private bool TankingIsUsingA { get; set; } = true;

        private List<Vector3> TankingKitingRouteA { get; } = new List<Vector3>()
        {
            new Vector3(3323, -3497, 287),
            new Vector3(3312, -3514, 287),
            new Vector3(3294, -3526, 287),
            new Vector3(3273, -3530, 287),
            new Vector3(3252, -3526, 287),
            new Vector3(3235, -3514, 287),
            new Vector3(3223, -3497, 287),
            new Vector3(3220, -3484, 287),
        };

        private List<Vector3> TankingKitingRouteB { get; } = new List<Vector3>()
        {
            new Vector3(3223, -3455, 287),
            new Vector3(3235, -3437, 287),
            new Vector3(3252, -3425, 287),
            new Vector3(3274, -3422, 287),
            new Vector3(3294, -3425, 287),
            new Vector3(3312, -3437, 287),
            new Vector3(3324, -3456, 287),
            new Vector3(3326, -3465, 287),
        };

        private Queue<Vector3> TankingPathQueue { get; }

        private Vector3 TankingSpotA { get; } = new Vector3(3325, -3486, 287);

        private Vector3 TankingSpotB { get; } = new Vector3(3222, -3464, 287);

        private bool TankIsKiting { get; set; } = false;

        private WowInterface WowInterface { get; }

        public bool ExecuteTactic(WowRole role, bool isMelee, out bool preventMovement, out bool allowAttacking)
        {
            return role switch
            {
                WowRole.Tank => DoTank(out preventMovement, out allowAttacking),
                WowRole.Heal => DoDpsHeal(false, out preventMovement, out allowAttacking),
                WowRole.Dps => DoDpsHeal(isMelee, out preventMovement, out allowAttacking),
                _ => throw new NotImplementedException(), // should never happen
            };
        }

        private bool DoDpsHeal(bool isMelee, out bool handlesMovement, out bool allowAttacking)
        {
            WowUnit wowUnit = WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(AnubRhekanDisplayId, false);

            if (wowUnit != null)
            {
                handlesMovement = true;
                allowAttacking = true;

                // Locust Swarm
                if (wowUnit.CurrentlyCastingSpellId == 28785)
                {
                    LocustSwarmActivated = DateTime.UtcNow;
                    WowInterface.CombatClass.BlacklistedTargetDisplayIds = AnubRhekanDisplayId;

                    MeleeDpsIsMovingToMid = true;
                }

                if (!LocustSwarmActive)
                {
                    WowInterface.CombatClass.BlacklistedTargetDisplayIds = null;
                }

                if (!isMelee)
                {
                    // Impale
                    if (wowUnit.CurrentlyCastingSpellId == 28783)
                    {
                        if (ImpaleDodgePos == Vector3.Zero)
                        {
                            float angle = new Random().NextDouble() > 0.5 ? MathF.PI + (MathF.PI / 2f) : MathF.PI - (MathF.PI / 2f);
                            ImpaleDodgePos = BotMath.CalculatePositionAround(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, angle, 2f);
                        }

                        WowInterface.MovementEngine.SetMovementAction(MovementAction.DirectMove, ImpaleDodgePos, 0);
                        return true;
                    }
                    else
                    {
                        ImpaleDodgePos = Vector3.Zero;
                    }

                    Vector3 targetPosition = BotUtils.MoveAhead(MiddleSpot, wowUnit.Position, -30.0f);

                    if (!LocustSwarmActive && WowInterface.ObjectManager.Player.Position.GetDistance(MiddleSpot) > 6.0)
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, targetPosition);
                        return true;
                    }
                }
                else
                {
                    if (MeleeDpsIsMovingToMid)
                    {
                        if (WowInterface.ObjectManager.Player.Position.GetDistance(MiddleSpot) > 24.0)
                        {
                            WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, MiddleSpot);
                            return true;
                        }
                        else
                        {
                            MeleeDpsIsMovingToMid = false;
                        }
                    }

                    handlesMovement = false;
                    allowAttacking = true;
                    return false;
                }
            }

            handlesMovement = false;
            allowAttacking = true;
            return false;
        }

        private bool DoTank(out bool handlesMovement, out bool allowAttacking)
        {
            WowUnit wowUnit = WowInterface.ObjectManager.GetClosestWowUnitByDisplayId(AnubRhekanDisplayId, false);

            if (wowUnit != null && wowUnit.TargetGuid == WowInterface.ObjectManager.PlayerGuid)
            {
                if (Configureables["isOffTank"] == true)
                {
                    // offtank should only focus adds
                    WowInterface.CombatClass.BlacklistedTargetDisplayIds = AnubRhekanDisplayId;
                }
                else
                {
                    WowInterface.CombatClass.BlacklistedTargetDisplayIds = AddsDisplayIds;
                    handlesMovement = true;
                    allowAttacking = true;

                    // Locust Swarm
                    if (wowUnit.CurrentlyCastingSpellId == 28785)
                    {
                        TankIsKiting = true;
                    }

                    if (!TankIsKiting)
                    {
                        Vector3 tankingSpot = TankingIsUsingA ? TankingSpotA : TankingSpotB;

                        if (WowInterface.ObjectManager.Player.Position.GetDistance2D(tankingSpot) > 2.0)
                        {
                            WowInterface.MovementEngine.SetMovementAction(MovementAction.DirectMove, tankingSpot, 0);
                        }
                    }
                    else
                    {
                        allowAttacking = false;

                        if (TankingPathQueue.Count == 0)
                        {
                            foreach (Vector3 v in TankingIsUsingA ? TankingKitingRouteA : TankingKitingRouteB)
                            {
                                TankingPathQueue.Enqueue(v);
                            }
                        }
                        else
                        {
                            Vector3 targetPosition = TankingPathQueue.Peek();

                            if (targetPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) > 2.0)
                            {
                                WowInterface.MovementEngine.SetMovementAction(MovementAction.DirectMove, targetPosition, 0);
                            }
                            else
                            {
                                TankingPathQueue.Dequeue();

                                if (TankingPathQueue.Count == 0)
                                {
                                    TankIsKiting = false;
                                    TankingIsUsingA = !TankingIsUsingA;
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            handlesMovement = false;
            allowAttacking = true;
            return false;
        }
    }
}