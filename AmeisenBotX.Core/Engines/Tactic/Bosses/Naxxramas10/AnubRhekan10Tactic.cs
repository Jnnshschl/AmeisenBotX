using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Tactic.Bosses.Naxxramas10
{
    public class AnubRhekan10Tactic : ITactic
    {
        public AnubRhekan10Tactic(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            TankingPathQueue = new();

            Configureables = new()
            {
                { "isOffTank", false },
            };
        }

        public Dictionary<string, dynamic> Configureables { get; private set; }

        public DateTime LocustSwarmActivated { get; private set; }

        private static List<int> AddsDisplayIds { get; } = new List<int> { 14698, 27943 };

        private static List<int> AnubRhekanDisplayId { get; } = new List<int> { 15931 };

        private AmeisenBotInterfaces Bot { get; }

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
            IWowUnit wowUnit = Bot.GetClosestQuestgiverByDisplayId(Bot.Player.Position, AnubRhekanDisplayId, false);

            if (wowUnit != null)
            {
                handlesMovement = true;
                allowAttacking = true;

                // Locust Swarm
                if (wowUnit.CurrentlyCastingSpellId == 28785)
                {
                    LocustSwarmActivated = DateTime.UtcNow;
                    Bot.CombatClass.BlacklistedTargetDisplayIds = AnubRhekanDisplayId;

                    MeleeDpsIsMovingToMid = true;
                }

                if (!LocustSwarmActive)
                {
                    Bot.CombatClass.BlacklistedTargetDisplayIds = null;
                }

                if (!isMelee)
                {
                    // Impale
                    if (wowUnit.CurrentlyCastingSpellId == 28783)
                    {
                        if (ImpaleDodgePos == Vector3.Zero)
                        {
                            float angle = new Random().NextDouble() > 0.5 ? MathF.PI + (MathF.PI / 2f) : MathF.PI - (MathF.PI / 2f);
                            ImpaleDodgePos = BotMath.CalculatePositionAround(Bot.Player.Position, Bot.Player.Rotation, angle, 2f);
                        }

                        Bot.Movement.SetMovementAction(MovementAction.DirectMove, ImpaleDodgePos, 0);
                        return true;
                    }
                    else
                    {
                        ImpaleDodgePos = Vector3.Zero;
                    }

                    Vector3 targetPosition = BotUtils.MoveAhead(MiddleSpot, wowUnit.Position, -30.0f);

                    if (!LocustSwarmActive && Bot.Player.Position.GetDistance(MiddleSpot) > 6.0)
                    {
                        Bot.Movement.SetMovementAction(MovementAction.Move, targetPosition);
                        return true;
                    }
                }
                else
                {
                    if (MeleeDpsIsMovingToMid)
                    {
                        if (Bot.Player.Position.GetDistance(MiddleSpot) > 24.0)
                        {
                            Bot.Movement.SetMovementAction(MovementAction.Move, MiddleSpot);
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
            IWowUnit wowUnit = Bot.GetClosestQuestgiverByDisplayId(Bot.Player.Position, AnubRhekanDisplayId, false);

            if (wowUnit != null && wowUnit.TargetGuid == Bot.Wow.PlayerGuid)
            {
                if (Configureables["isOffTank"] == true)
                {
                    // offtank should only focus adds
                    Bot.CombatClass.BlacklistedTargetDisplayIds = AnubRhekanDisplayId;
                }
                else
                {
                    Bot.CombatClass.BlacklistedTargetDisplayIds = AddsDisplayIds;
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

                        if (Bot.Player.Position.GetDistance2D(tankingSpot) > 2.0)
                        {
                            Bot.Movement.SetMovementAction(MovementAction.DirectMove, tankingSpot, 0);
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

                            if (targetPosition.GetDistance2D(Bot.Player.Position) > 2.0)
                            {
                                Bot.Movement.SetMovementAction(MovementAction.DirectMove, targetPosition, 0);
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